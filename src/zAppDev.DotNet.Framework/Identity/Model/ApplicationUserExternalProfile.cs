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
    /// The ApplicationUserExternalProfile class
    ///
    /// </summary>
    [Serializable]
    [DataContract]
    public class ApplicationUserExternalProfile : IDomainModelClass
    {
        #region ApplicationUserExternalProfile's Fields

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
        [DataMember(Name="Gender")]
        protected string gender;
        [DataMember(Name="Name")]
        protected string name;
        [DataMember(Name="Surname")]
        protected string surname;
        [DataMember(Name="DisplayName")]
        protected string displayName;
        [DataMember(Name="Email")]
        protected string email;
        [DataMember(Name="Provider")]
        protected string provider;

        #endregion
        #region ApplicationUserExternalProfile's Properties
/// <summary>
/// The Id property
///The ID of this person
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
/// The Gender property
///The person's gender. Possible values: Male, Female, Other
/// </summary>
///
        public virtual string Gender
        {
            get
            {
                return gender;
            }
            set
            {
                gender = value;
            }
        }
/// <summary>
/// The Name property
///The given name (first name) of this person
/// </summary>
///
        public virtual string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
/// <summary>
/// The Surname property
///The family name (last name) of this person
/// </summary>
///
        public virtual string Surname
        {
            get
            {
                return surname;
            }
            set
            {
                surname = value;
            }
        }
/// <summary>
/// The DisplayName property
///The name of this person, which is suitable for display.
/// </summary>
///
        public virtual string DisplayName
        {
            get
            {
                return displayName;
            }
            set
            {
                displayName = value;
            }
        }
/// <summary>
/// The Email property
///The E-Mail of this person
/// </summary>
///
        public virtual string Email
        {
            get
            {
                return email;
            }
            set
            {
                email = value;
            }
        }
/// <summary>
/// The Provider property
///The Provider of the External Profile
/// </summary>
///
        public virtual string Provider
        {
            get
            {
                return provider;
            }
            set
            {
                provider = value;
            }
        }

        #endregion
        #region Constructors
/// <summary>
/// Public constructors of the ApplicationUserExternalProfile class
/// </summary>
/// <returns>New ApplicationUserExternalProfile object</returns>
/// <remarks></remarks>
        public ApplicationUserExternalProfile() {}
        #endregion
        #region Methods

        public virtual List<string> _Validate(bool throwException = true)
        {
            var __errors = new List<string>();
            if (Id == null)
            {
                __errors.Add("Property 'Id' is required.");
            }
            if (Gender != null && Gender.Length > 100)
            {
                __errors.Add("Length of property 'Gender' cannot be greater than 100.");
            }
            if (Name != null && Name.Length > 1000)
            {
                __errors.Add("Length of property 'Name' cannot be greater than 1000.");
            }
            if (Surname != null && Surname.Length > 1000)
            {
                __errors.Add("Length of property 'Surname' cannot be greater than 1000.");
            }
            if (DisplayName != null && DisplayName.Length > 1000)
            {
                __errors.Add("Length of property 'DisplayName' cannot be greater than 1000.");
            }
            if (Email != null && Email.Length > 1000)
            {
                __errors.Add("Length of property 'Email' cannot be greater than 1000.");
            }
            if (Provider != null && Provider.Length > 1000)
            {
                __errors.Add("Length of property 'Provider' cannot be greater than 1000.");
            }
            if (throwException && __errors.Any())
            {
                throw new BusinessException("An instance of TypeClass 'ApplicationUserExternalProfile' has validation errors:\r\n\r\n" + string.Join("\r\n", __errors));
            }
            return __errors;
        }

        public virtual int _GetUniqueIdentifier()
        {
            var hashCode = 399326290;
            hashCode = hashCode * -1521134295 + (Id?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Gender?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Name?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Surname?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (DisplayName?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Email?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Provider?.GetHashCode() ?? 0);
            return hashCode;
        }






/// <summary>
/// Copies the current object to a new instance
/// </summary>
/// <param name="deep">Copy members that refer to objects external to this class (not dependent)</param>
/// <param name="copiedObjects">Objects that should be reused</param>
/// <param name="asNew">Copy the current object as a new one, ready to be persisted, along all its members.</param>
/// <param name="reuseNestedObjects">If asNew is true, this flag if set, forces the reuse of all external objects.</param>
/// <param name="copy">Optional - An existing [ApplicationUserExternalProfile] instance to use as the destination.</param>
/// <returns>A copy of the object</returns>
        public virtual ApplicationUserExternalProfile Copy(bool deep=false, Hashtable copiedObjects=null, bool asNew=false, bool reuseNestedObjects = false, ApplicationUserExternalProfile copy = null)
        {
            if(copiedObjects == null)
            {
                copiedObjects = new Hashtable();
            }
            if (copy == null && copiedObjects.Contains(this))
                return (ApplicationUserExternalProfile)copiedObjects[this];
            copy = copy ?? new ApplicationUserExternalProfile();
            if (!asNew)
            {
                copy.TransientId = this.TransientId;
                copy.Id = this.Id;
            }
            copy.Gender = this.Gender;
            copy.Name = this.Name;
            copy.Surname = this.Surname;
            copy.DisplayName = this.DisplayName;
            copy.Email = this.Email;
            copy.Provider = this.Provider;
            if (!copiedObjects.Contains(this))
            {
                copiedObjects.Add(this, copy);
            }
            return copy;
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as ApplicationUserExternalProfile;
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
        public static bool operator ==(ApplicationUserExternalProfile x, ApplicationUserExternalProfile y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Equals(x, y);
        }

// Maintain inequality operator semantics for entities.
        public static bool operator !=(ApplicationUserExternalProfile x, ApplicationUserExternalProfile y)
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
        protected bool HasSameNonDefaultIdAs(ApplicationUserExternalProfile compareTo)
        {
            return !this.IsTransient() && !compareTo.IsTransient() && this.Id.Equals(compareTo.Id);
        }

        #endregion


    }
}
