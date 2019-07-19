using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

#if NETFRAMEWORK
using System.Web;
using CLMS.Framework.Owin;
using CLMS.AppDev.Cache;
using Microsoft.AspNet.Identity.Owin;
#else
using CLMS.Framework.Utilities;
#endif

namespace CLMS.Framework.Mvc
{
    public class ViewModelDTOBase
    {
        [JsonConstructor]
        public ViewModelDTOBase() { }

        public object _clientKey { get; set; }
        public Dictionary<string, bool> _clientPostedProps;
        protected Dictionary<string, bool> _dirtyState;
        protected bool _parentIsDirty;
        public List<string> _updatedProperties
        {
            get
            {
                return _dirtyState == null ? null : _dirtyState.Select(kv => kv.Key).ToList();
            }
        }
        
        public virtual object _key { get; }
		public virtual string _originalTypeClassName { get; set; }
        public virtual List<string> _baseClasses { get; set; }
        public virtual object _typeHash => GetType().Name.GetHashCode();

        public string Serialize(JsonSerializerSettings settings = null, bool indented = false)
        {
            return Utilities.Serialize(this, settings, indented);
        }

#if NETFRAMEWORK

        public static DTOHelper DTOHelper
        {
            get
            {
                return OwinHelper.GetOwinContext(HttpContext.Current)?.Get<DTOHelper>();
            }

            set
            {
                var helper = OwinHelper.GetOwinContext(HttpContext.Current)?.Get<DTOHelper>();

                if (helper != null)
                {
                    throw new InvalidOperationException("Do not set the DTO Helper when another one exists in the OWIN context!");
                }

                OwinHelper.GetOwinContext(HttpContext.Current)?.Set(value);
            }
        }

#else
        public static DTOHelper DTOHelper => (DTOHelper)ServiceLocator.Current.GetInstance<IDTOHelper>();
#endif

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as ViewModelDTOBase;

            // same reference -> EQUAL
            if (ReferenceEquals(this, compareTo)) return true;

            // other is null -> NOT EQUAL
            if (compareTo == null) return false;

            // not of same original type -> NOT EQUAL
            // TODO: Check inheritence
            if (_originalTypeClassName != compareTo._originalTypeClassName) return false;

            // At this point we know that we compare dtos of the same original type

            // same client key -> EQUAL
            if (_clientKey == compareTo._clientKey) return true;

            // same non default keys -> EQUAL
            // TODO: more sofisticated code is needed here
            if (_key != null && !string.IsNullOrWhiteSpace(_key.ToString()) &&
                _key.ToString() != "0" &&
                _key.ToString() == compareTo.ToString())
            {
                return true;
            }

            // any other case -> NOT EQUAL
            return false;
        }
    }

    public class DTOHelper
    {
        private static string _nonPersistedDataset = "NonPersistedDataSetQueryables";
   
        public DTOHelper()
        {
            SeenModelInstances = new Dictionary<object, object>();
            SeenDTOInstances = new Dictionary<object, object>();
        }
        
        public Dictionary<object, object> SeenModelInstances;

        public Dictionary<object, object> SeenDTOInstances;

        public T GetDTOFromModel<T>(object original, int level = 0, int maxLevel = 4) where T : class
        {
            if (level >= maxLevel)
            {
                return null;
            }

            if (SeenDTOInstances.ContainsKey(original))
            {
                var seen = SeenDTOInstances[original] as T;

                if (seen != null) return seen;
            }

            var dtoType = typeof(T);
            var originalType = original.GetType();
            var originalKey = originalType.GetProperties().SingleOrDefault(prop => Attribute.IsDefined(prop, typeof(KeyAttribute)));
            var originalKeyValue = originalKey == null
                                    ? null
                                    : originalKey.GetValue(original, null);

            var newDTOInstance = (T)Activator.CreateInstance(dtoType, original, true);
            var clientKeyProperty = newDTOInstance.GetType().GetProperty("_clientKey");
            var key = GetClientKey(original, originalKeyValue);

            clientKeyProperty.SetValue(newDTOInstance, key);

            return (T)newDTOInstance;
        }

        public T GetModelFromDTO<T>(IViewModelDTO<T> dtoInstance, int level = 0, int maxLevel = 4) where T : class
        {
            if (dtoInstance == null ||  level >= maxLevel) return null;                

            if (SeenModelInstances.ContainsKey(dtoInstance))
            {
                var seen = SeenModelInstances[dtoInstance] as T;

                if (seen != null) return seen;
            }

            var newModelInstance = dtoInstance.Convert();

            return (T)newModelInstance;
        }

        public object GetClientKey(object instance, object keyValue)
        {
            if (SeenModelInstances.ContainsValue(instance))
            {
                var seenDto = SeenModelInstances.FirstOrDefault(v => v.Value == instance).Key as ViewModelDTOBase;

                if (seenDto != null)
                {

                    var key = seenDto._clientKey;

                    if (key != null) return key;
                }
            }

            return keyValue == null || keyValue.ToString() == "0" || keyValue.ToString() == "" || keyValue.ToString() == new Guid().ToString()
                ? Math.Abs(instance.GetHashCode())
                : keyValue;
        }
		
		 public T GetSeenModelInstance<T>(object clientKey, string type, List<string> baseClasses  = null) where T : class
        {
            return SeenModelInstances.FirstOrDefault(x => 
                            (x.Key as ViewModelDTOBase)._clientKey?.Equals(clientKey) == true &&
                            ((x.Key as ViewModelDTOBase)._originalTypeClassName?.Equals(type) == true ||
							(baseClasses != null && baseClasses.Any(z => (x.Key as ViewModelDTOBase)._originalTypeClassName?.Equals(z) == true)))).Value as T;            
        }
		
		public void UpdateSeenModelInstances(object dto, object original)
		{
            if (SeenModelInstances.ContainsKey(dto))
			{
                SeenModelInstances[dto] = original;
			}
			else
			{
				SeenModelInstances.Add(dto, original);
			}
		}

        public Dictionary<string, Dictionary<string, bool>> _clientPostedProperties 
            = new Dictionary<string, Dictionary<string, bool>>();


        internal bool DidClientPosted(string type, object instanceId, string property)
        {
            if (instanceId == null)
            {
                return false;
            }

            var key = type + "_" + instanceId;

            if (!_clientPostedProperties.ContainsKey(key))
            {
                return true;
            }

            return _clientPostedProperties[key].ContainsKey(property);
        }

        internal void ClientPostedProps(string type, object instanceId, Dictionary<string, bool> clientPostedProps)
        {
            if (instanceId == null || clientPostedProps == null)
            {
                return;
            }

            var key = type + "_" + instanceId;

            if (!_clientPostedProperties.ContainsKey(key))
            {
                _clientPostedProperties.Add(key, new Dictionary<string, bool>());
            }

            var entries = _clientPostedProperties[key];

            foreach (var prop in clientPostedProps)
            {
                if (entries.ContainsKey(prop.Key))
                {
                    continue;
                }
                entries.Add(prop.Key, true);
            }
        }
    }

    public class SelectedItemInfo<T>
    {
        [JsonConstructor]
        public SelectedItemInfo() { }

        public SelectedItemInfo(List<T> selectedItems, string indexes, bool full)
        {
            SelectedItems = selectedItems.Where(i => i != null).ToList();
            Indexes = indexes;
            FullRecordsetSelected = full;
        }

        public List<T> SelectedItems;
        public bool FullRecordsetSelected;
        public string Indexes;

        private static SelectedItemInfo<Y> GetSelectionForIndex<Y>(List<SelectedItemInfo<Y>> allIndexesData, string indexes = null)
        {
            return string.IsNullOrEmpty(indexes)
                ? allIndexesData.FirstOrDefault(l => string.IsNullOrEmpty(l.Indexes) || l.Indexes == "_")
                : allIndexesData.FirstOrDefault(l => l.Indexes.Equals(indexes));
        }

        public static List<Y> GetSelectedItems<Y>(IQueryable<Y> collection, List<SelectedItemInfo<Y>> allIndexesData, string indexes = null)
        {
            var selectionForThisIndex = GetSelectionForIndex(allIndexesData, indexes);

            if (selectionForThisIndex == null) return new List<Y>();

            if (selectionForThisIndex.FullRecordsetSelected)
            {
                selectionForThisIndex.SelectedItems = collection.ToList(); // no filters, no paging
            }

            return selectionForThisIndex.SelectedItems;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class OriginalType : Attribute
    {
        public Type Type;

        public OriginalType(Type type)
        {
            Type = type;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DataSetDTO : Attribute
    {

    }
}