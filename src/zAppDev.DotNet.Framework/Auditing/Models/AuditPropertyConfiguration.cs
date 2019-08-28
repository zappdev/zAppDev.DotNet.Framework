using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using zAppDev.DotNet.Framework.Data.Domain;
using zAppDev.DotNet.Framework.Data.DAL;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Auditing.Model
{
    /// <summary>
    /// The AuditPropertyConfiguration class
    ///
    /// </summary>
    [Serializable]
    [DataContract]
    public class AuditPropertyConfiguration : IDomainModelClass
    {
        protected const int HashMultiplier = 31;
        protected int? cachedHashcode;

        protected Guid _transientId = Guid.NewGuid();
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

        /// <summary>
        /// The Id property
        ///
        /// </summary>
        ///
        [Key]
        [DataMember(Name = "Id")]
        public virtual int? Id { get; set; } = 0;

        /// <summary>   
        /// The Name property
        ///
        /// </summary>
        ///
        [DataMember(Name = "Name")]
        public virtual string Name { get; set; }

        /// <summary>
        /// The DataType property
        ///
        /// </summary>
        ///
        [DataMember(Name = "DataType")]
        public virtual string DataType { get; set; }

        /// <summary>
        /// The IsAuditable property
        ///
        /// </summary>
        ///
        [DataMember(Name = "IsAuditable")]
        public virtual bool IsAuditable { get; set; }

        /// <summary>
        /// The IsComplex property
        ///
        /// </summary>
        ///
        [DataMember(Name = "IsComplex")]
        public virtual bool IsComplex { get; set; }

        /// <summary>
        /// The IsCollection property
        ///
        /// </summary>
        ///
        [DataMember(Name = "IsCollection")]
        public virtual bool IsCollection { get; set; }

        /// <summary>
        /// The VersionTimestamp property
        ///Provides concurrency control for the class
        /// </summary>
        ///
        [DataMember(Name = "VersionTimestamp")]
        public virtual byte[] VersionTimestamp { get; set; }

        [DataMember(Name="Entity")]
        protected AuditEntityConfiguration entity;
        public virtual AuditEntityConfiguration Entity
        {
            get
            {
                return entity;
            }
            set
            {
                if(Equals(entity, value)) return;
                entity?.InternalRemoveProperties(this);
                entity = value;
                if (value != null)
                {
                    entity.InternalAddProperties(this);
                }
            }
        }

        /// <summary>
        /// Public constructors of the AuditPropertyConfiguration class
        /// </summary>
        /// <returns>New AuditPropertyConfiguration object</returns>
        /// <remarks></remarks>
        public AuditPropertyConfiguration()
        {

        }

        public virtual List<string> _Validate(bool throwException = true)
        {
            var __errors = new List<string>();
            if (Id == null)
            {
                __errors.Add("Property 'Id' is required.");
            }
            if (Name != null && Name.Length > 4000)
            {
                __errors.Add("Length of property 'Name' cannot be greater than 4000.");
            }
            if (DataType != null && DataType.Length > 4000)
            {
                __errors.Add("Length of property 'DataType' cannot be greater than 4000.");
            }
            if (Entity == null)
            {
                __errors.Add("Association with 'Entity' is required.");
            }
            if (throwException && __errors.Any())
            {
                throw new Exceptions.BusinessException("An instance of TypeClass 'AuditPropertyConfiguration' has validation errors:\r\n\r\n" + string.Join("\r\n", __errors));
            }
            return __errors;
        }

        public virtual int _GetUniqueIdentifier()
        {
            var hashCode = 399326290;
            hashCode = hashCode * -1521134295 + (Id?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Name?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (DataType?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + IsAuditable.GetHashCode() ;
            hashCode = hashCode * -1521134295 + IsComplex.GetHashCode() ;
            hashCode = hashCode * -1521134295 + IsCollection.GetHashCode() ;
            return hashCode;
        }

        /// <summary>
        /// Copies the current object to a new instance
        /// </summary>
        /// <param name="deep">Copy members that refer to objects external to this class (not dependent)</param>
        /// <param name="copiedObjects">Objects that should be reused</param>
        /// <param name="asNew">Copy the current object as a new one, ready to be persisted, along all its members.</param>
        /// <param name="reuseNestedObjects">If asNew is true, this flag if set, forces the reuse of all external objects.</param>
        /// <param name="copy">Optional - An existing [AuditPropertyConfiguration] instance to use as the destination.</param>
        /// <returns>A copy of the object</returns>
        public virtual AuditPropertyConfiguration Copy(bool deep=false, Hashtable copiedObjects=null, bool asNew=false, bool reuseNestedObjects = false, AuditPropertyConfiguration copy = null)
        {
            if(copiedObjects == null)
            {
                copiedObjects = new Hashtable();
            }
            if (copy == null && copiedObjects.Contains(this))
                return (AuditPropertyConfiguration)copiedObjects[this];
            copy = copy ?? new AuditPropertyConfiguration();
            if (!asNew)
            {
                copy.TransientId = TransientId;
                copy.Id = Id;
            }
            copy.Name = Name;
            copy.DataType = DataType;
            copy.IsAuditable = IsAuditable;
            copy.IsComplex = IsComplex;
            copy.IsCollection = IsCollection;
            if (!copiedObjects.Contains(this))
            {
                copiedObjects.Add(this, copy);
            }
            if(deep && entity != null)
            {
                if (!copiedObjects.Contains(entity))
                {
                    if (asNew && reuseNestedObjects)
                        copy.Entity = Entity;
                    else if (asNew)
                        copy.Entity = Entity.Copy(deep, copiedObjects, true);
                    else
                        copy.entity = entity.Copy(deep, copiedObjects, false);
                }
                else
                {
                    if (asNew)
                        copy.Entity = (AuditEntityConfiguration)copiedObjects[Entity];
                    else
                        copy.entity = (AuditEntityConfiguration)copiedObjects[Entity];
                }
            }
            return copy;
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as AuditPropertyConfiguration;
            if (ReferenceEquals(this, compareTo))
            {
                return true;
            }
            if (compareTo == null || !GetType().Equals(compareTo.GetTypeUnproxied()))
            {
                return false;
            }
            if (HasSameNonDefaultIdAs(compareTo))
            {
                return true;
            }
            // Since the Ids aren't the same, both of them must be transient to
            // compare domain signatures; because if one is transient and the
            // other is a persisted entity, then they cannot be the same object.
            return IsTransient() && compareTo.IsTransient() && (base.Equals(compareTo) || TransientId.Equals(compareTo.TransientId));
        }

        // Maintain equality operator semantics for entities.
        public static bool operator ==(AuditPropertyConfiguration x, AuditPropertyConfiguration y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Equals(x, y);
        }

        // Maintain inequality operator semantics for entities.
        public static bool operator !=(AuditPropertyConfiguration x, AuditPropertyConfiguration y)
        {
            return !(x == y);
        }

        private PropertyInfo __propertyKeyCache;
        public virtual PropertyInfo GetPrimaryKey()
        {
            if (__propertyKeyCache == null)
            {
                __propertyKeyCache = GetType().GetProperty("Id");
            }
            return __propertyKeyCache;
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
        protected virtual Type GetTypeUnproxied()
        {
            return GetType();
        }

        /// <summary>
        ///     To help ensure hashcode uniqueness, a carefully selected random number multiplier
        ///     is used within the calculation.  Goodrich and Tamassia's Data Structures and
        ///     Algorithms in Java asserts that 31, 33, 37, 39 and 41 will produce the fewest number
        ///     of collissions.  See http://computinglife.wordpress.com/2008/11/20/why-do-hash-functions-use-prime-numbers/
        ///     for more information.
        /// </summary>
        public override int GetHashCode()
        {
            if (cachedHashcode.HasValue)
            {
                return cachedHashcode.Value;
            }
            if (IsTransient())
            {
                //this.cachedHashcode = base.GetHashCode();
                return TransientId.GetHashCode(); //don't cache because this won't stay transient forever
            }
            else
            {
                unchecked
                {
                    // It's possible for two objects to return the same hash code based on
                    // identically valued properties, even if they're of two different types,
                    // so we include the object's type in the hash calculation
                    var hashCode = GetType().GetHashCode();
                    cachedHashcode = (hashCode * HashMultiplier) ^ Id.GetHashCode();
                }
            }
            return cachedHashcode.Value;
        }

        /// <summary>
        ///     Transient objects are not associated with an item already in storage.  For instance,
        ///     a Customer is transient if its Id is 0.  It's virtual to allow NHibernate-backed
        ///     objects to be lazily loaded.
        /// </summary>
        public virtual bool IsTransient()
        {
            return Id == default(int) || Id.Equals(default(int));
        }
        
        /// <summary>
        ///     Returns true if self and the provided entity have the same Id values
        ///     and the Ids are not of the default Id value
        /// </summary>
        protected bool HasSameNonDefaultIdAs(AuditPropertyConfiguration compareTo)
        {
            return !IsTransient() && !compareTo.IsTransient() && Id.Equals(compareTo.Id);
        }

        public virtual void UpdateAuditPropertyConfiguration(AuditPropertyConfiguration tmp)
        {
#if NETFRAMEWORK
            using (new Profiling.Profiler(nameof(AuditEntityConfiguration), Profiling.AppDevSymbolType.ClassOperation, nameof(AuditEntityConfiguration.SkipEntity)))
            {
#endif
            IsAuditable = tmp?.IsAuditable ?? false;
            IsCollection = tmp?.IsCollection ?? false;
            IsComplex = tmp?.IsComplex ?? false;
            DataType = tmp?.DataType ?? "";
#if NETFRAMEWORK
        }
#endif
        }

        public static List<AuditPropertyConfiguration> GetAuditEntityProperties(Type runtimeEntityProperty)
        {
#if NETFRAMEWORK
            using (new Profiling.Profiler(nameof(AuditPropertyConfiguration), Profiling.AppDevSymbolType.ClassOperation, "GetAuditEntityProperties"))
            {
#endif
                var repo = ServiceLocator.Current.GetInstance<IRepositoryBuilder>().CreateRetrieveRepository();

                List<AuditPropertyConfiguration> properties = new List<AuditPropertyConfiguration>();
                AuditPropertyConfiguration newproperty = new AuditPropertyConfiguration();
                List<AuditEntityConfiguration> existingEntities = repo.GetAll<AuditEntityConfiguration>();
                AuditEntityConfiguration existingEntity = new AuditEntityConfiguration();
                foreach (var currentProperty in MambaRuntimeType.FromPropertiesList(runtimeEntityProperty.GetProperties()) ?? Enumerable.Empty<MambaRuntimeType>())
                {
                    if ((SkipProperty(currentProperty.Name)))
                    {
                        continue;
                    }
                    newproperty = new AuditPropertyConfiguration();
                    existingEntity = existingEntities?.FirstOrDefault((a) => a.FullName == Common.GetTypeName(runtimeEntityProperty, true));
                    newproperty.Name = currentProperty.Name;
                    newproperty.IsComplex = ((Common.IsPropertyPrimitiveOrSimple(currentProperty)) == false);
                    if ((currentProperty.PropertyType.GenericTypeArguments.Length > 0) && (currentProperty.PropertyType.GenericTypeArguments.ToList().FirstOrDefault() != null))
                    {
                        newproperty.DataType = Common.GetTypeName(currentProperty.PropertyType.GenericTypeArguments.ToList().FirstOrDefault(), true);
                    }
                    else
                    {
                        newproperty.DataType = Common.GetTypeName(currentProperty.PropertyType, true);
                    }
                    if (newproperty?.DataType == "System.String")
                    {
                        newproperty.IsCollection = false;
                    }
                    else
                    {
                        newproperty.IsCollection = Common.IsPropertyCollection(currentProperty);
                    }
                    if (existingEntity != null && existingEntity?.Properties?.FirstOrDefault((x) => x.Name == currentProperty.Name) != null)
                    {
                        newproperty.IsAuditable = (existingEntity?.Properties?.FirstOrDefault((x) => x.Name == currentProperty.Name)?.IsAuditable ?? false);
                    }
                    else
                    {
                        newproperty.IsAuditable = false;
                    }
                    properties?.Add(newproperty);
                }
                return properties;
#if NETFRAMEWORK
            }
#endif
        }

        public static bool SkipProperty(string property)
        {
            return property == "TransientId" 
                || property == "DbTimestamp" 
                || property == "VersionTimestamp";
        }
    }
}
