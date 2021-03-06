// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using zAppDev.DotNet.Framework.Data.Domain;
using zAppDev.DotNet.Framework.Utilities;
using zAppDev.DotNet.Framework.Data.DAL;

namespace zAppDev.DotNet.Framework.Auditing.Model
{
    /// <summary>
    /// The AuditEntityConfiguration class
    ///
    /// </summary>
    [Serializable]
    [DataContract]
    public class AuditEntityConfiguration : IDomainModelClass
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

#pragma warning disable 0649
        private bool disableInternalAdditions;
#pragma warning restore 0649

        /// <summary>
        /// The Id property
        ///
        /// </summary>
        ///
        [Key]
        [DataMember(Name = "Id")]
        public virtual int? Id { get; set; } = 0;

        /// <summary>
        /// The FullName property
        ///
        /// </summary>
        ///
        [DataMember(Name = "FullName")]
        public virtual string FullName { get; set; }

        /// <summary>
        /// The ShortName property
        ///
        /// </summary>
        ///
        [DataMember(Name = "ShortName")]
        public virtual string ShortName { get; set; }

        /// <summary>
        /// The VersionTimestamp property
        ///Provides concurrency control for the class
        /// </summary>
        ///
        [DataMember(Name = "VersionTimestamp")]
        public virtual int? VersionTimestamp { get; set; }

        [DataMember(Name = "Properties")]
        protected IList<AuditPropertyConfiguration> properties = new List<AuditPropertyConfiguration>();
        public virtual List<AuditPropertyConfiguration> Properties
        {
            get
            {
                if (properties is AuditPropertyConfiguration[])
                {
                    properties = properties.ToList();
                }
                if (properties == null)
                {
                    properties = new List<AuditPropertyConfiguration>();
                }
                return properties.ToList();
            }
            set
            {
                if (properties is AuditPropertyConfiguration[])
                {
                    properties = properties.ToList();
                }
                if (properties != null)
                {
                    var __itemsToDelete = new List<AuditPropertyConfiguration>(properties);
                    foreach (var __item in __itemsToDelete)
                    {
                        RemoveProperties(__item);
                    }
                }
                if (value == null)
                {
                    properties = new List<AuditPropertyConfiguration>();
                    return;
                }
                foreach (var __item in value)
                {
                    AddProperties(__item);
                }
            }
        }
        public virtual void AddProperties(IList<AuditPropertyConfiguration> __items)
        {
            foreach (var __item in __items)
            {
                AddProperties(__item);
            }
        }

        public virtual void InternalAddProperties(AuditPropertyConfiguration __item)
        {
            if (__item == null || disableInternalAdditions) return;
            properties?.Add(__item);
        }

        public virtual void InternalRemoveProperties(AuditPropertyConfiguration __item)
        {
            if (__item == null) return;
            properties?.Remove(__item);
        }

        public virtual void AddProperties(AuditPropertyConfiguration __item)
        {
            if (__item == null) return;
            if (__item.Entity != this)
                __item.Entity = this;
        }

        public virtual void AddAtIndexProperties(int index, AuditPropertyConfiguration __item)
        {
            if (__item == null) return;
            properties?.Insert(index, __item);
            disableInternalAdditions = true;
            try
            {
                if (__item.Entity != this)
                    __item.Entity = this;
            }
            finally
            {
                disableInternalAdditions = false;
            }
        }

        public virtual void RemoveProperties(AuditPropertyConfiguration __item)
        {
            if (__item != null)
            {
                __item.Entity = null;
            }
        }
        public virtual void SetPropertiesAt(AuditPropertyConfiguration __item, int __index)
        {
            if (__item == null)
            {
                properties[__index].Entity = null;
            }
            else
            {
                properties[__index] = __item;
                if (__item.Entity != this)
                    __item.Entity = this;
            }
        }

        public virtual void ClearProperties()
        {
            if (properties != null)
            {
                var __itemsToRemove = properties.ToList();
                foreach (var __item in __itemsToRemove)
                {
                    RemoveProperties(__item);
                }
            }
        }

        /// <summary>
        /// Public constructors of the AuditEntityConfiguration class
        /// </summary>
        /// <returns>New AuditEntityConfiguration object</returns>
        /// <remarks></remarks>
        public AuditEntityConfiguration() { }

        public virtual List<string> _Validate(bool throwException = true)
        {
            var __errors = new List<string>();
            if (Id == null)
            {
                __errors.Add("Property 'Id' is required.");
            }
            if (FullName != null && FullName.Length > 4000)
            {
                __errors.Add("Length of property 'FullName' cannot be greater than 4000.");
            }
            if (ShortName != null && ShortName.Length > 4000)
            {
                __errors.Add("Length of property 'ShortName' cannot be greater than 4000.");
            }
            if (throwException && __errors.Any())
            {
                throw new Exceptions.BusinessException("An instance of TypeClass 'AuditEntityConfiguration' has validation errors:\r\n\r\n" + string.Join("\r\n", __errors));
            }
            return __errors;
        }

        public virtual int _GetUniqueIdentifier()
        {
            var hashCode = 399326290;
            hashCode = hashCode * -1521134295 + (Id?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (FullName?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (ShortName?.GetHashCode() ?? 0);
            return hashCode;
        }
        
        /// <summary>
        /// Copies the current object to a new instance
        /// </summary>
        /// <param name="deep">Copy members that refer to objects external to this class (not dependent)</param>
        /// <param name="copiedObjects">Objects that should be reused</param>
        /// <param name="asNew">Copy the current object as a new one, ready to be persisted, along all its members.</param>
        /// <param name="reuseNestedObjects">If asNew is true, this flag if set, forces the reuse of all external objects.</param>
        /// <param name="copy">Optional - An existing [AuditEntityConfiguration] instance to use as the destination.</param>
        /// <returns>A copy of the object</returns>
        public virtual AuditEntityConfiguration Copy(bool deep = false, Hashtable copiedObjects = null, bool asNew = false, bool reuseNestedObjects = false, AuditEntityConfiguration copy = null)
        {
            if (copiedObjects == null)
            {
                copiedObjects = new Hashtable();
            }
            if (copy == null && copiedObjects.Contains(this))
                return (AuditEntityConfiguration)copiedObjects[this];
            copy = copy ?? new AuditEntityConfiguration();
            if (!asNew)
            {
                copy.TransientId = TransientId;
                copy.Id = Id;
            }
            copy.FullName = FullName;
            copy.ShortName = ShortName;
            if (!copiedObjects.Contains(this))
            {
                copiedObjects.Add(this, copy);
            }
            copy.properties = new List<AuditPropertyConfiguration>();
            if (deep && properties != null)
            {
                foreach (var __item in properties)
                {
                    if (!copiedObjects.Contains(__item))
                    {
                        if (asNew && reuseNestedObjects)
                            copy.AddProperties(__item);
                        else
                            copy.AddProperties(__item.Copy(deep, copiedObjects, asNew));
                    }
                    else
                    {
                        copy.AddProperties((AuditPropertyConfiguration)copiedObjects[__item]);
                    }
                }
            }
            return copy;
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as AuditEntityConfiguration;
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
        public static bool operator ==(AuditEntityConfiguration x, AuditEntityConfiguration y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Equals(x, y);
        }

        // Maintain inequality operator semantics for entities.
        public static bool operator !=(AuditEntityConfiguration x, AuditEntityConfiguration y)
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
        protected bool HasSameNonDefaultIdAs(AuditEntityConfiguration compareTo)
        {
            return !IsTransient() && !compareTo.IsTransient() && Id.Equals(compareTo.Id);
        }

        public static bool SkipEntity(string entity)
        {

#if NETFRAMEWORK
            using (new Profiling.Profiler(nameof(AuditEntityConfiguration), Profiling.AppDevSymbolType.ClassOperation, nameof(AuditEntityConfiguration.SkipEntity)))
            {
#endif
            var list = new List<string>
            {
                "IDomainModelClass",
                "BusinessException"
            };
            if (list.Contains(entity))
            {
                return true;
            }
            return false;
#if NETFRAMEWORK
            }
#endif
        }

        public virtual void UpdateAuditEntityConfiguration(AuditEntityConfiguration newEntityConfiguration, IAuditingRepository repo = null)
        {
            repo = repo ?? ServiceLocator.Current.GetInstance<IRepositoryBuilder>().CreateAuditingRepository();

#if NETFRAMEWORK
            using (new Profiling.Profiler(nameof(AuditEntityConfiguration), Profiling.AppDevSymbolType.ClassOperation, nameof(AuditEntityConfiguration.UpdateAuditEntityConfiguration)))
            {
#endif
            var newPropertyConfiguration = new AuditPropertyConfiguration();
            var newProperties = newEntityConfiguration.Properties;

            Properties?.ForEach(oldPropertyConfiguration =>
            {
                newPropertyConfiguration = newProperties?
                    .FirstOrDefault(a => a.Name == oldPropertyConfiguration?.Name);
                if (newPropertyConfiguration == null)
                {
                    repo.DeleteAuditPropertyConfiguration(oldPropertyConfiguration);
                }
                else
                {
                    oldPropertyConfiguration?.UpdateAuditPropertyConfiguration(newPropertyConfiguration);
                    newProperties?.RemoveAll(c => c.Name == newPropertyConfiguration.Name);
                }
            });

            if (newProperties.Any())
            {
                this?.AddProperties(newProperties);
            }

            repo.Save(this);
#if NETFRAMEWORK
            }
#endif
        }

        internal static void SetAuditableTypes(List<Type> auditableTypes)
        {
            _auditableTypes = auditableTypes;
        }

        internal static Type GetTypeFromClassName(string name)
        {            
            return _auditableTypes.FirstOrDefault(c => c.FullName == name);
        }

        private static List<Type> _auditableTypes = new List<Type>();

        public static List<AuditEntityConfiguration> GetAllEntityConfigurations()
        {
#if NETFRAMEWORK
           using (new Profiling.Profiler(nameof(AuditEntityConfiguration), Profiling.AppDevSymbolType.ClassOperation, nameof(AuditEntityConfiguration.GetAllEntityConfigurations)))
           {
#endif
            var entities = new List<AuditEntityConfiguration>();

            foreach (var currentClassType in _auditableTypes)
            {
                if (SkipEntity(Common.GetTypeName(currentClassType, false)))
                {
                    continue;
                }

                var newEntity = new AuditEntityConfiguration();
                newEntity = new AuditEntityConfiguration
                {
                    FullName = Common.GetTypeName(currentClassType, true),
                    ShortName = Common.GetTypeName(currentClassType, false),
                    Properties = AuditPropertyConfiguration.GetAuditEntityProperties(currentClassType).ToList()
                };
                entities?.Add(newEntity);
            }
            return entities;
#if NETFRAMEWORK
            }
#endif            
        }

    }
}
