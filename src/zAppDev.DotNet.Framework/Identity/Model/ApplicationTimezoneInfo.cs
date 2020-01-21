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

namespace zAppDev.DotNet.Framework.Identity.Model
{
    /// <summary>
    /// The ApplicationLanguage class
    ///
    /// </summary>
    [Serializable]
    [DataContract]
    public class ApplicationTimezoneInfo : IDomainModelClass
    {
        #region ApplicationTimezonInfo's Fields

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
        [DataMember(Name = "Id")]
        protected string id;
        [DataMember(Name = "DisplayName")]
        protected string displayName;
        [DataMember(Name = "StandardName")]
        protected string standardName;
        [DataMember(Name = "BaseUtcOffset")]
        protected TimeSpan baseUtcOffset;
        [DataMember(Name = "VersionTimestamp")]
        protected byte[] versionTimestamp;

        #endregion
        #region ApplicationTimezonInfo's Properties
        /// <summary>
        /// The Id property
        ///
        /// </summary>
        ///
        [Key]
        public virtual string Id
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
        /// The DisplayName property
        ///
        /// </summary>
        ///
        public virtual string DisplayName
        {
            get
            {
                return DisplayName;
            }
            set
            {
                DisplayName = value;
            }
        }
        /// <summary>
        /// The StandardName property
        ///
        /// </summary>
        ///
        public virtual string StandardName
        {
            get
            {
                return StandardName;
            }
            set
            {
                StandardName = value;
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
        #region ApplicationTimezonInfo's Participant Properties       
        public virtual TimeSpan BaseUtcOffset
        {
            get
            {
                return baseUtcOffset;
            }
            set
            {
                if (Equals(baseUtcOffset, value)) return;
                baseUtcOffset = value;
            }
        }
        #endregion
        #region Constructors
        /// <summary>
        /// Public constructors of the ApplicationTimezonInfo class
        /// </summary>
        /// <returns>New ApplicationTimezonInfo object</returns>
        /// <remarks></remarks>
        public ApplicationTimezoneInfo() { }
        #endregion
        #region Methods

        public virtual List<string> _Validate(bool throwException = true)
        {
            var __errors = new List<string>();
            if (Id == null)
            {
                __errors.Add("Property 'Id' is required.");
            }
            if (DisplayName != null && DisplayName.Length > 100)
            {
                __errors.Add("Length of property 'DisplayName' cannot be greater than 100.");
            }
            if (StandardName != null && StandardName.Length > 100)
            {
                __errors.Add("Length of property 'StandardName' cannot be greater than 100.");
            }
            if (BaseUtcOffset == null)
            {
                __errors.Add("Property BaseUtcOffset is required");
            }
            if (throwException && __errors.Any())
            {
                throw new BusinessException("An instance of TypeClass 'ApplicationTimezonInfo' has validation errors:\r\n\r\n" + string.Join("\r\n", __errors));
            }
            return __errors;
        }

        public virtual int _GetUniqueIdentifier()
        {
            var hashCode = 399326290;
            hashCode = hashCode * -1521134295 + (Id?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (DisplayName?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (StandardName?.GetHashCode() ?? 0);
            return hashCode;
        }






        /// <summary>
        /// Copies the current object to a new instance
        /// </summary>
        /// <param name="deep">Copy members that refer to objects external to this class (not dependent)</param>
        /// <param name="copiedObjects">Objects that should be reused</param>
        /// <param name="asNew">Copy the current object as a new one, ready to be persisted, along all its members.</param>
        /// <param name="reuseNestedObjects">If asNew is true, this flag if set, forces the reuse of all external objects.</param>
        /// <param name="copy">Optional - An existing [ApplicationLanguage] instance to use as the destination.</param>
        /// <returns>A copy of the object</returns>
        public virtual ApplicationTimezoneInfo Copy(bool deep = false, Hashtable copiedObjects = null, bool asNew = false, bool reuseNestedObjects = false, ApplicationTimezoneInfo copy = null)
        {
            if (copiedObjects == null)
            {
                copiedObjects = new Hashtable();
            }
            if (copy == null && copiedObjects.Contains(this))
                return (ApplicationTimezoneInfo)copiedObjects[this];
            copy = copy ?? new ApplicationTimezoneInfo();
            if (!asNew)
            {
                copy.TransientId = this.TransientId;
                copy.Id = this.Id;
            }
            copy.DisplayName = this.DisplayName;
            copy.StandardName= this.StandardName;
            
            if (!copiedObjects.Contains(this))
            {
                copiedObjects.Add(this, copy);
            }
            if (deep && this.baseUtcOffset!= null)
            {
                copy.BaseUtcOffset = this.BaseUtcOffset;
            }
            return copy;
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as ApplicationTimezoneInfo;
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
        public static bool operator ==(ApplicationTimezoneInfo x, ApplicationTimezoneInfo y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Equals(x, y);
        }

        // Maintain inequality operator semantics for entities.
        public static bool operator !=(ApplicationTimezoneInfo x, ApplicationTimezoneInfo y)
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
            return this.Id == default || this.Id.Equals(default);
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
            return this.GetType();
        }

        /// <summary>
        ///     Returns true if self and the provided entity have the same Id values
        ///     and the Ids are not of the default Id value
        /// </summary>
        protected bool HasSameNonDefaultIdAs(ApplicationTimezoneInfo compareTo)
        {
            return !this.IsTransient() && !compareTo.IsTransient() && this.Id.Equals(compareTo.Id);
        }

        #endregion
    }
}
