// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using zAppDev.DotNet.Framework.Linq;
using zAppDev.DotNet.Framework.Data.Domain;
using zAppDev.DotNet.Framework.Workflow;
using zAppDev.DotNet.Framework.Exceptions;

namespace zAppDev.DotNet.Framework.Identity.Model
{
    /// <summary>
    /// The ApplicationUserClaim class
    ///
    /// </summary>
    [Serializable]
    [DataContract]
    public class ApplicationUserClaim : IDomainModelClass
    {
        #region ApplicationUserClaim's Fields

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
        [DataMember(Name="ClaimType")]
        protected string claimType;
        [DataMember(Name="ClaimValue")]
        protected string claimValue;
        [DataMember(Name="ClaimValueType")]
        protected string claimValueType;
        [DataMember(Name="Issuer")]
        protected string issuer;
        [DataMember(Name="OriginalIssuer")]
        protected string originalIssuer;
        [DataMember(Name="VersionTimestamp")]
        protected int? versionTimestamp;

        #endregion
        #region ApplicationUserClaim's Properties
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
/// The ClaimType property
///
/// </summary>
///
        public virtual string ClaimType
        {
            get
            {
                return claimType;
            }
            set
            {
                claimType = value;
            }
        }
/// <summary>
/// The ClaimValue property
///
/// </summary>
///
        public virtual string ClaimValue
        {
            get
            {
                return claimValue;
            }
            set
            {
                claimValue = value;
            }
        }
/// <summary>
/// The ClaimValueType property
///
/// </summary>
///
        public virtual string ClaimValueType
        {
            get
            {
                return claimValueType;
            }
            set
            {
                claimValueType = value;
            }
        }
/// <summary>
/// The Issuer property
///
/// </summary>
///
        public virtual string Issuer
        {
            get
            {
                return issuer;
            }
            set
            {
                issuer = value;
            }
        }
/// <summary>
/// The OriginalIssuer property
///
/// </summary>
///
        public virtual string OriginalIssuer
        {
            get
            {
                return originalIssuer;
            }
            set
            {
                originalIssuer = value;
            }
        }
/// <summary>
/// The VersionTimestamp property
///Provides concurrency control for the class
/// </summary>
///
        public virtual int? VersionTimestamp
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
        #region ApplicationUserClaim's Participant Properties
        [DataMember(Name="User")]
        protected ApplicationUser user;
        public virtual ApplicationUser User
        {
            get
            {
                return user;
            }
            set
            {
                if(Equals(user, value)) return;
                var __oldValue = user;
                user?.InternalRemoveClaims(this);
                user = value;
                if (value != null)
                {
                    user.InternalAddClaims(this);
                }
            }
        }
        #endregion
        #region Constructors
/// <summary>
/// Public constructors of the ApplicationUserClaim class
/// </summary>
/// <returns>New ApplicationUserClaim object</returns>
/// <remarks></remarks>
        public ApplicationUserClaim() {}
        #endregion
        #region Methods

        public virtual List<string> _Validate(bool throwException = true)
        {
            var __errors = new List<string>();
            if (Id == null)
            {
                __errors.Add("Property 'Id' is required.");
            }
            if (ClaimType != null && ClaimType.Length > 2147483647)
            {
                __errors.Add("Length of property 'ClaimType' cannot be greater than 2147483647.");
            }
            if (ClaimValue != null && ClaimValue.Length > 2147483647)
            {
                __errors.Add("Length of property 'ClaimValue' cannot be greater than 2147483647.");
            }
            if (ClaimValueType != null && ClaimValueType.Length > 100)
            {
                __errors.Add("Length of property 'ClaimValueType' cannot be greater than 100.");
            }
            if (Issuer != null && Issuer.Length > 2147483647)
            {
                __errors.Add("Length of property 'Issuer' cannot be greater than 2147483647.");
            }
            if (OriginalIssuer != null && OriginalIssuer.Length > 2147483647)
            {
                __errors.Add("Length of property 'OriginalIssuer' cannot be greater than 2147483647.");
            }
            if (User == null)
            {
                __errors.Add("Association with 'User' is required.");
            }
            if (throwException && __errors.Any())
            {
                throw new BusinessException("An instance of TypeClass 'ApplicationUserClaim' has validation errors:\r\n\r\n" + string.Join("\r\n", __errors));
            }
            return __errors;
        }

        public virtual int _GetUniqueIdentifier()
        {
            var hashCode = 399326290;
            hashCode = hashCode * -1521134295 + (Id?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (ClaimType?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (ClaimValue?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (ClaimValueType?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Issuer?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (OriginalIssuer?.GetHashCode() ?? 0);
            return hashCode;
        }






/// <summary>
/// Copies the current object to a new instance
/// </summary>
/// <param name="deep">Copy members that refer to objects external to this class (not dependent)</param>
/// <param name="copiedObjects">Objects that should be reused</param>
/// <param name="asNew">Copy the current object as a new one, ready to be persisted, along all its members.</param>
/// <param name="reuseNestedObjects">If asNew is true, this flag if set, forces the reuse of all external objects.</param>
/// <param name="copy">Optional - An existing [ApplicationUserClaim] instance to use as the destination.</param>
/// <returns>A copy of the object</returns>
        public virtual ApplicationUserClaim Copy(bool deep=false, Hashtable copiedObjects=null, bool asNew=false, bool reuseNestedObjects = false, ApplicationUserClaim copy = null)
        {
            if(copiedObjects == null)
            {
                copiedObjects = new Hashtable();
            }
            if (copy == null && copiedObjects.Contains(this))
                return (ApplicationUserClaim)copiedObjects[this];
            copy = copy ?? new ApplicationUserClaim();
            if (!asNew)
            {
                copy.TransientId = this.TransientId;
                copy.Id = this.Id;
            }
            copy.ClaimType = this.ClaimType;
            copy.ClaimValue = this.ClaimValue;
            copy.ClaimValueType = this.ClaimValueType;
            copy.Issuer = this.Issuer;
            copy.OriginalIssuer = this.OriginalIssuer;
            if (!copiedObjects.Contains(this))
            {
                copiedObjects.Add(this, copy);
            }
            if(deep && this.user != null)
            {
                if (!copiedObjects.Contains(this.user))
                {
                    if (asNew && reuseNestedObjects)
                        copy.User = this.User;
                    else if (asNew)
                        copy.User = this.User.Copy(deep, copiedObjects, true);
                    else
                        copy.user = this.user.Copy(deep, copiedObjects, false);
                }
                else
                {
                    if (asNew)
                        copy.User = (ApplicationUser)copiedObjects[this.User];
                    else
                        copy.user = (ApplicationUser)copiedObjects[this.User];
                }
            }
            return copy;
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as ApplicationUserClaim;
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
        public static bool operator ==(ApplicationUserClaim x, ApplicationUserClaim y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Equals(x, y);
        }

// Maintain inequality operator semantics for entities.
        public static bool operator !=(ApplicationUserClaim x, ApplicationUserClaim y)
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
        protected bool HasSameNonDefaultIdAs(ApplicationUserClaim compareTo)
        {
            return !this.IsTransient() && !compareTo.IsTransient() && this.Id.Equals(compareTo.Id);
        }

        #endregion


    }
}
