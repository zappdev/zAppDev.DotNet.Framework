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
using zAppDev.DotNet.Framework.Exceptions;

namespace zAppDev.DotNet.Framework.Auditing.Model
{
    /// <summary>
    /// The AuditLogEntry class
    ///
    /// </summary>
    [Serializable]
    [DataContract]
    public class AuditLogEntry : IDomainModelClass
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
        /// The UserName property
        ///
        /// </summary>
        ///
        [DataMember(Name = "UserName")]
        public virtual string UserName { get; set; }

        /// <summary>
        /// The IPAddress property
        ///
        /// </summary>
        ///
        [DataMember(Name = "IPAddress")]
        public virtual string IPAddress { get; set; }

        /// <summary>
        /// The EntityFullName property
        ///
        /// </summary>
        ///
        [DataMember(Name = "EntityFullName")]
        public virtual string EntityFullName { get; set; }

        /// <summary>
        /// The EntityShortName property
        ///
        /// </summary>
        ///
        [DataMember(Name = "EntityShortName")]
        public virtual string EntityShortName { get; set; }

        /// <summary>
        /// The EntityId property
        ///
        /// </summary>
        ///
        [DataMember(Name = "EntityId")]
        public virtual string EntityId { get; set; }

        /// <summary>
        /// The Timestamp property
        ///
        /// </summary>
        ///
        [DataMember(Name = "Timestamp")]
        public virtual DateTime? Timestamp { get; set; }

        /// <summary>
        /// The EntryTypeId property
        ///
        /// </summary>
        ///
        [DataMember(Name = "EntryTypeId")]
        public virtual int? EntryTypeId { get; set; }

        /// <summary>
        /// The ActionTypeId property
        ///
        /// </summary>
        ///
        [DataMember(Name = "ActionTypeId")]
        public virtual int? ActionTypeId { get; set; }

        /// <summary>
        /// The OldValue property
        ///
        /// </summary>
        ///
        [DataMember(Name = "OldValue")]
        public virtual string OldValue { get; set; }

        /// <summary>
        /// The NewValue property
        ///
        /// </summary>
        ///
        [DataMember(Name = "NewValue")]
        public virtual string NewValue { get; set; }

        /// <summary>
        /// The PropertyName property
        ///
        /// </summary>
        ///
        [DataMember(Name = "PropertyName")]
        public virtual string PropertyName { get; set; }

        /// <summary>
        /// The ExtraField1 property
        ///
        /// </summary>
        ///
        [DataMember(Name = "ExtraField1")]
        public virtual string ExtraField1 { get; set; }

        /// <summary>
        /// The ExtraField2 property
        ///
        /// </summary>
        ///
        [DataMember(Name = "ExtraField2")]
        public virtual string ExtraField2 { get; set; }

        /// <summary>
        /// The ExtraField3 property
        ///
        /// </summary>
        ///
        [DataMember(Name = "ExtraField3")]
        public virtual string ExtraField3 { get; set; }

        /// <summary>
        /// Public constructors of the AuditLogEntry class
        /// </summary>
        /// <returns>New AuditLogEntry object</returns>
        /// <remarks></remarks>
        public AuditLogEntry() {}

        public virtual List<string> _Validate(bool throwException = true)
        {
            var __errors = new List<string>();
            if (Id == null)
            {
                __errors.Add("Property 'Id' is required.");
            }
            if (UserName != null && UserName.Length > 4000)
            {
                __errors.Add("Length of property 'UserName' cannot be greater than 4000.");
            }
            if (IPAddress != null && IPAddress.Length > 4000)
            {
                __errors.Add("Length of property 'IPAddress' cannot be greater than 4000.");
            }
            if (EntityFullName != null && EntityFullName.Length > 4000)
            {
                __errors.Add("Length of property 'EntityFullName' cannot be greater than 4000.");
            }
            if (EntityShortName != null && EntityShortName.Length > 4000)
            {
                __errors.Add("Length of property 'EntityShortName' cannot be greater than 4000.");
            }
            if (EntityId != null && EntityId.Length > 4000)
            {
                __errors.Add("Length of property 'EntityId' cannot be greater than 4000.");
            }
            if (OldValue != null && OldValue.Length > 4000)
            {
                __errors.Add("Length of property 'OldValue' cannot be greater than 4000.");
            }
            if (NewValue != null && NewValue.Length > 4000)
            {
                __errors.Add("Length of property 'NewValue' cannot be greater than 4000.");
            }
            if (PropertyName != null && PropertyName.Length > 4000)
            {
                __errors.Add("Length of property 'PropertyName' cannot be greater than 4000.");
            }
            if (throwException && __errors.Any())
            {
                throw new BusinessException("An instance of TypeClass 'AuditLogEntry' has validation errors:\r\n\r\n" + string.Join("\r\n", __errors));
            }
            return __errors;
        }

        public virtual int _GetUniqueIdentifier()
        {
            var hashCode = 399326290;
            hashCode = hashCode * -1521134295 + (Id?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (UserName?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (IPAddress?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (EntityFullName?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (EntityShortName?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (EntityId?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Timestamp?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (EntryTypeId?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (ActionTypeId?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (OldValue?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (NewValue?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (PropertyName?.GetHashCode() ?? 0);
            return hashCode;
        }

        /// <summary>
        /// Copies the current object to a new instance
        /// </summary>
        /// <param name="deep">Copy members that refer to objects external to this class (not dependent)</param>
        /// <param name="copiedObjects">Objects that should be reused</param>
        /// <param name="asNew">Copy the current object as a new one, ready to be persisted, along all its members.</param>
        /// <param name="reuseNestedObjects">If asNew is true, this flag if set, forces the reuse of all external objects.</param>
        /// <param name="copy">Optional - An existing [AuditLogEntry] instance to use as the destination.</param>
        /// <returns>A copy of the object</returns>
        public virtual AuditLogEntry Copy(bool deep=false, Hashtable copiedObjects=null, bool asNew=false, bool reuseNestedObjects = false, AuditLogEntry copy = null)
        {
            if(copiedObjects == null)
            {
                copiedObjects = new Hashtable();
            }
            if (copy == null && copiedObjects.Contains(this))
                return (AuditLogEntry)copiedObjects[this];
            copy = copy ?? new AuditLogEntry();
            if (!asNew)
            {
                copy.TransientId = TransientId;
                copy.Id = Id;
            }
            copy.UserName = UserName;
            copy.IPAddress = IPAddress;
            copy.EntityFullName = EntityFullName;
            copy.EntityShortName = EntityShortName;
            copy.EntityId = EntityId;
            copy.Timestamp = Timestamp;
            copy.EntryTypeId = EntryTypeId;
            copy.ActionTypeId = ActionTypeId;
            copy.OldValue = OldValue;
            copy.NewValue = NewValue;
            copy.PropertyName = PropertyName;
            if (!copiedObjects.Contains(this))
            {
                copiedObjects.Add(this, copy);
            }
            return copy;
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as AuditLogEntry;
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
        public static bool operator ==(AuditLogEntry x, AuditLogEntry y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Equals(x, y);
        }

        // Maintain inequality operator semantics for entities.
        public static bool operator !=(AuditLogEntry x, AuditLogEntry y)
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
        protected bool HasSameNonDefaultIdAs(AuditLogEntry compareTo)
        {
            return !IsTransient() && !compareTo.IsTransient() && Id.Equals(compareTo.Id);
        }
    }
}
