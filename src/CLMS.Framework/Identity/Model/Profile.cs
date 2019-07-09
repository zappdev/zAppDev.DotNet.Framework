using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using CLMS.Framework.Linq;
using CLMS.Framework.Data.Domain;
using CLMS.Framework.Workflow;

namespace CLMS.Framework.Identity.Model
{
    /// <summary>
    /// The Profile class
    ///
    /// </summary>
    [Serializable]
    [DataContract]
    public class Profile : IDomainModelClass
    {
        #region Profile's Fields

        protected Guid _transientId= Guid.NewGuid();
        public virtual Guid TransientId
        {
            get
            {
                return _transientId;
            }
            set
            {
                _transientId = value;
            }
        }
        [DataMember(Name="Id")]
        protected int? id = 0;
        [DataMember(Name="LanguageLCID")]
        protected int? languageLCID;
        [DataMember(Name="LocaleLCID")]
        protected int? localeLCID;
        [DataMember(Name="Theme")]
        protected string theme;
        [DataMember(Name="VersionTimestamp")]
        protected byte[] versionTimestamp;

#pragma warning disable 0649
        private bool disableInternalAdditions;
#pragma warning restore 0649
        #endregion
        #region Profile's Properties
/// <summary>
/// The Id property
///
/// </summary>
///
        [Key]
        public virtual int? Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
/// <summary>
/// The LanguageLCID property
///
/// </summary>
///
        public virtual int? LanguageLCID
        {
            get
            {
                return languageLCID;
            }
            set
            {
                languageLCID = value;
            }
        }
/// <summary>
/// The LocaleLCID property
///
/// </summary>
///
        public virtual int? LocaleLCID
        {
            get
            {
                return localeLCID;
            }
            set
            {
                localeLCID = value;
            }
        }
/// <summary>
/// The Theme property
///
/// </summary>
///
        public virtual string Theme
        {
            get
            {
                return theme;
            }
            set
            {
                theme = value;
            }
        }
/// <summary>
/// The VersionTimestamp property
///Provides concurrency control for the class
/// </summary>
///
        public virtual byte[] VersionTimestamp
        {
            get
            {
                return versionTimestamp;
            }
            set
            {
                versionTimestamp = value;
            }
        }
        #endregion
        #region Profile's Participant Properties
        [DataMember(Name="Settings")]
        protected IList<ProfileSetting> settings = new List<ProfileSetting>();
        public virtual List<ProfileSetting> Settings
        {
            get
            {
                if (settings is ProfileSetting[])
                {
                    settings = settings.ToList();
                }
                if (settings == null)
                {
                    settings = new List<ProfileSetting>();
                }
                return settings.ToList();
            }
            set
            {
                if (settings is ProfileSetting[])
                {
                    settings = settings.ToList();
                }
                if (settings != null)
                {
                    var __itemsToDelete = new List<ProfileSetting>(settings);
                    foreach (var __item in __itemsToDelete)
                    {
                        RemoveSettings(__item);
                    }
                }
                if(value == null)
                {
                    settings = new List<ProfileSetting>();
                    return;
                }
                foreach(var __item in value)
                {
                    AddSettings(__item);
                }
            }
        }
        public virtual void AddSettings(IList<ProfileSetting> __items)
        {
            foreach (var __item in __items)
            {
                AddSettings(__item);
            }
        }

        public virtual void InternalAddSettings(ProfileSetting __item)
        {
            if (__item == null || disableInternalAdditions) return;
            settings?.Add(__item);
        }

        public virtual void InternalRemoveSettings(ProfileSetting __item)
        {
            if (__item == null) return;
            settings?.Remove(__item);
        }

        public virtual void AddSettings(ProfileSetting __item)
        {
            if (__item == null) return;
            if (__item.ParentProfile != this)
                __item.ParentProfile = this;
        }

        public virtual void AddAtIndexSettings(int index, ProfileSetting __item)
        {
            if (__item == null) return;
            settings?.Insert(index, __item);
            disableInternalAdditions = true;
            try
            {
                if (__item.ParentProfile != this)
                    __item.ParentProfile = this;
            }
            finally
            {
                disableInternalAdditions = false;
            }
        }

        public virtual void RemoveSettings(ProfileSetting __item)
        {
            if (__item != null)
            {
                __item.ParentProfile = null;
            }
        }
        public virtual void SetSettingsAt(ProfileSetting __item, int __index)
        {
            if (__item == null)
            {
                settings[__index].ParentProfile = null;
            }
            else
            {
                settings[__index] = __item;
                if (__item.ParentProfile != this)
                    __item.ParentProfile = this;
            }
        }

        public virtual void ClearSettings()
        {
            if (settings!=null)
            {
                var __itemsToRemove = settings.ToList();
                foreach(var __item in __itemsToRemove)
                {
                    RemoveSettings(__item);
                }
            }
        }
        #endregion
        #region Constructors
/// <summary>
/// Public constructors of the Profile class
/// </summary>
/// <returns>New Profile object</returns>
/// <remarks></remarks>
        public Profile() {}
        #endregion
        #region Methods

        public virtual List<string> _Validate(bool throwException = true)
        {
            var __errors = new List<string>();
            if (Id == null)
            {
                __errors.Add("Property 'Id' is required.");
            }
            if (Theme != null && Theme.Length > 100)
            {
                __errors.Add("Length of property 'Theme' cannot be greater than 100.");
            }
            if (throwException && __errors.Any())
            {
                throw new CLMS.Framework.Exceptions.BusinessException("An instance of TypeClass 'Profile' has validation errors:\r\n\r\n" + string.Join("\r\n", __errors));
            }
            return __errors;
        }

        public virtual int _GetUniqueIdentifier()
        {
            var hashCode = 399326290;
            hashCode = hashCode * -1521134295 + (Id?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (LanguageLCID?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (LocaleLCID?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Theme?.GetHashCode() ?? 0);
            return hashCode;
        }






/// <summary>
/// Copies the current object to a new instance
/// </summary>
/// <param name="deep">Copy members that refer to objects external to this class (not dependent)</param>
/// <param name="copiedObjects">Objects that should be reused</param>
/// <param name="asNew">Copy the current object as a new one, ready to be persisted, along all its members.</param>
/// <param name="reuseNestedObjects">If asNew is true, this flag if set, forces the reuse of all external objects.</param>
/// <param name="copy">Optional - An existing [Profile] instance to use as the destination.</param>
/// <returns>A copy of the object</returns>
        public virtual Profile Copy(bool deep=false, Hashtable copiedObjects=null, bool asNew=false, bool reuseNestedObjects = false, Profile copy = null)
        {
            if(copiedObjects == null)
            {
                copiedObjects = new Hashtable();
            }
            if (copy == null && copiedObjects.Contains(this))
                return (Profile)copiedObjects[this];
            copy = copy ?? new Profile();
            if (!asNew)
            {
                copy.TransientId = this.TransientId;
                copy.Id = this.Id;
            }
            copy.LanguageLCID = this.LanguageLCID;
            copy.LocaleLCID = this.LocaleLCID;
            copy.Theme = this.Theme;
            if (!copiedObjects.Contains(this))
            {
                copiedObjects.Add(this, copy);
            }
            copy.settings = new List<ProfileSetting>();
            if(deep && this.settings != null)
            {
                foreach (var __item in this.settings)
                {
                    if (!copiedObjects.Contains(__item))
                    {
                        if (asNew && reuseNestedObjects)
                            copy.AddSettings(__item);
                        else
                            copy.AddSettings(__item.Copy(deep, copiedObjects, asNew));
                    }
                    else
                    {
                        copy.AddSettings((ProfileSetting)copiedObjects[__item]);
                    }
                }
            }
            return copy;
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as Profile;
            if (ReferenceEquals(this, compareTo))
            {
                return true;
            }
            if (compareTo == null || !this.GetType().Equals(compareTo.GetTypeUnproxied()))
            {
                return false;
            }
            if (this.HasSameNonDefaultIdAs(compareTo))
            {
                return true;
            }
            // Since the Ids aren't the same, both of them must be transient to
            // compare domain signatures; because if one is transient and the
            // other is a persisted entity, then they cannot be the same object.
            return this.IsTransient() && compareTo.IsTransient() && (base.Equals(compareTo) || this.TransientId.Equals(compareTo.TransientId));
        }

// Maintain equality operator semantics for entities.
        public static bool operator ==(Profile x, Profile y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Equals(x, y);
        }

// Maintain inequality operator semantics for entities.
        public static bool operator !=(Profile x, Profile y)
        {
            return !(x == y);
        }

        private PropertyInfo __propertyKeyCache;
        public virtual PropertyInfo GetPrimaryKey()
        {
            if (__propertyKeyCache == null)
            {
                __propertyKeyCache = this.GetType().GetProperty("Id");
            }
            return __propertyKeyCache;
        }


/// <summary>
///     To help ensure hashcode uniqueness, a carefully selected random number multiplier
///     is used within the calculation.  Goodrich and Tamassia's Data Structures and
///     Algorithms in Java asserts that 31, 33, 37, 39 and 41 will produce the fewest number
///     of collissions.  See http://computinglife.wordpress.com/2008/11/20/why-do-hash-functions-use-prime-numbers/
///     for more information.
/// </summary>
        private const int HashMultiplier = 31;
        private int? cachedHashcode;

        public override int GetHashCode()
        {
            if (this.cachedHashcode.HasValue)
            {
                return this.cachedHashcode.Value;
            }
            if (this.IsTransient())
            {
                //this.cachedHashcode = base.GetHashCode();
                return this.TransientId.GetHashCode(); //don't cache because this won't stay transient forever
            }
            else
            {
                unchecked
                {
                    // It's possible for two objects to return the same hash code based on
                    // identically valued properties, even if they're of two different types,
                    // so we include the object's type in the hash calculation
                    var hashCode = this.GetType().GetHashCode();
                    this.cachedHashcode = (hashCode * HashMultiplier) ^ this.Id.GetHashCode();
                }
            }
            return this.cachedHashcode.Value;
        }

/// <summary>
///     Transient objects are not associated with an item already in storage.  For instance,
///     a Customer is transient if its Id is 0.  It's virtual to allow NHibernate-backed
///     objects to be lazily loaded.
/// </summary>
        public virtual bool IsTransient()
        {
            return this.Id == default(int) || this.Id.Equals(default(int));
        }

/// <summary>
///     When NHibernate proxies objects, it masks the type of the actual entity object.
///     This wrapper burrows into the proxied object to get its actual type.
///
///     Although this assumes NHibernate is being used, it doesn't require any NHibernate
///     related dependencies and has no bad side effects if NHibernate isn't being used.
///
///     Related discussion is at http://groups.google.com/group/sharp-architecture/browse_thread/thread/ddd05f9baede023a ...thanks Jay Oliver!
/// </summary>
        protected virtual System.Type GetTypeUnproxied()
        {
            return this.GetType();
        }

/// <summary>
///     Returns true if self and the provided entity have the same Id values
///     and the Ids are not of the default Id value
/// </summary>
        protected bool HasSameNonDefaultIdAs(Profile compareTo)
        {
            return !this.IsTransient() && !compareTo.IsTransient() && this.Id.Equals(compareTo.Id);
        }

        #endregion


    }
}
