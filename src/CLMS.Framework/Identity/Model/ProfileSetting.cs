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
    /// The ProfileSetting class
    ///
    /// </summary>
    [Serializable]
    [DataContract]
    public class ProfileSetting : IDomainModelClass
    {
        #region ProfileSetting's Fields

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
        [DataMember(Name="Key")]
        protected string key;
        [DataMember(Name="Value")]
        protected string _value;
        [DataMember(Name="VersionTimestamp")]
        protected byte[] versionTimestamp;

        #endregion
        #region ProfileSetting's Properties
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
/// The Key property
///
/// </summary>
///
        public virtual string Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
            }
        }
/// <summary>
/// The Value property
///
/// </summary>
///
        public virtual string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
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
        #region ProfileSetting's Participant Properties
        [DataMember(Name="ParentProfile")]
        protected Profile parentProfile;
        public virtual Profile ParentProfile
        {
            get
            {
                return parentProfile;
            }
            set
            {
                if(Equals(parentProfile, value)) return;
                var __oldValue = parentProfile;
                parentProfile?.InternalRemoveSettings(this);
                parentProfile = value;
                if (value != null)
                {
                    parentProfile.InternalAddSettings(this);
                }
            }
        }
        #endregion
        #region Constructors
/// <summary>
/// Public constructors of the ProfileSetting class
/// </summary>
/// <returns>New ProfileSetting object</returns>
/// <remarks></remarks>
        public ProfileSetting() {}
        #endregion
        #region Methods

        public virtual List<string> _Validate(bool throwException = true)
        {
            var __errors = new List<string>();
            if (Id == null)
            {
                __errors.Add("Property 'Id' is required.");
            }
            if (Key != null && Key.Length > 100)
            {
                __errors.Add("Length of property 'Key' cannot be greater than 100.");
            }
            if (Value != null && Value.Length > 2147483647)
            {
                __errors.Add("Length of property 'Value' cannot be greater than 2147483647.");
            }
            if (ParentProfile == null)
            {
                __errors.Add("Association with 'ParentProfile' is required.");
            }
            if (throwException && __errors.Any())
            {
                throw new CLMS.Framework.Exceptions.BusinessException("An instance of TypeClass 'ProfileSetting' has validation errors:\r\n\r\n" + string.Join("\r\n", __errors));
            }
            return __errors;
        }

        public virtual int _GetUniqueIdentifier()
        {
            var hashCode = 399326290;
            hashCode = hashCode * -1521134295 + (Id?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Key?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Value?.GetHashCode() ?? 0);
            return hashCode;
        }






/// <summary>
/// Copies the current object to a new instance
/// </summary>
/// <param name="deep">Copy members that refer to objects external to this class (not dependent)</param>
/// <param name="copiedObjects">Objects that should be reused</param>
/// <param name="asNew">Copy the current object as a new one, ready to be persisted, along all its members.</param>
/// <param name="reuseNestedObjects">If asNew is true, this flag if set, forces the reuse of all external objects.</param>
/// <param name="copy">Optional - An existing [ProfileSetting] instance to use as the destination.</param>
/// <returns>A copy of the object</returns>
        public virtual ProfileSetting Copy(bool deep=false, Hashtable copiedObjects=null, bool asNew=false, bool reuseNestedObjects = false, ProfileSetting copy = null)
        {
            if(copiedObjects == null)
            {
                copiedObjects = new Hashtable();
            }
            if (copy == null && copiedObjects.Contains(this))
                return (ProfileSetting)copiedObjects[this];
            copy = copy ?? new ProfileSetting();
            if (!asNew)
            {
                copy.TransientId = this.TransientId;
                copy.Id = this.Id;
            }
            copy.Key = this.Key;
            copy.Value = this.Value;
            if (!copiedObjects.Contains(this))
            {
                copiedObjects.Add(this, copy);
            }
            if(deep && this.parentProfile != null)
            {
                if (!copiedObjects.Contains(this.parentProfile))
                {
                    if (asNew && reuseNestedObjects)
                        copy.ParentProfile = this.ParentProfile;
                    else if (asNew)
                        copy.ParentProfile = this.ParentProfile.Copy(deep, copiedObjects, true);
                    else
                        copy.parentProfile = this.parentProfile.Copy(deep, copiedObjects, false);
                }
                else
                {
                    if (asNew)
                        copy.ParentProfile = (Profile)copiedObjects[this.ParentProfile];
                    else
                        copy.parentProfile = (Profile)copiedObjects[this.ParentProfile];
                }
            }
            return copy;
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as ProfileSetting;
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
        public static bool operator ==(ProfileSetting x, ProfileSetting y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Equals(x, y);
        }

// Maintain inequality operator semantics for entities.
        public static bool operator !=(ProfileSetting x, ProfileSetting y)
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
        protected bool HasSameNonDefaultIdAs(ProfileSetting compareTo)
        {
            return !this.IsTransient() && !compareTo.IsTransient() && this.Id.Equals(compareTo.Id);
        }

        #endregion


    }
}
