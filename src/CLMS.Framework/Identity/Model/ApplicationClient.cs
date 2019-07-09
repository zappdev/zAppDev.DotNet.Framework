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
    /// The ApplicationClient class
    ///
    /// </summary>
    [Serializable]
    [DataContract]
    public class ApplicationClient : IDomainModelClass
    {
        #region ApplicationClient's Fields

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
        [DataMember(Name="ClientKey")]
        protected string clientKey;
        [DataMember(Name="IPAddress")]
        protected string iPAddress;
        [DataMember(Name="SessionId")]
        protected string sessionId;
        [DataMember(Name="ConnectedOn")]
        protected DateTime? connectedOn;
        [DataMember(Name="VersionTimestamp")]
        protected byte[] versionTimestamp;

        #endregion
        #region ApplicationClient's Properties
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
/// The ClientKey property
///
/// </summary>
///
        public virtual string ClientKey
        {
            get
            {
                return clientKey;
            }
            set
            {
                clientKey = value;
            }
        }
/// <summary>
/// The IPAddress property
///
/// </summary>
///
        public virtual string IPAddress
        {
            get
            {
                return iPAddress;
            }
            set
            {
                iPAddress = value;
            }
        }
/// <summary>
/// The SessionId property
///
/// </summary>
///
        public virtual string SessionId
        {
            get
            {
                return sessionId;
            }
            set
            {
                sessionId = value;
            }
        }
/// <summary>
/// The ConnectedOn property
///
/// </summary>
///
        public virtual DateTime? ConnectedOn
        {
            get
            {
                return connectedOn;
            }
            set
            {
                connectedOn = value;
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
        #region ApplicationClient's Participant Properties
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
                user?.InternalRemoveClients(this);
                user = value;
                if (value != null)
                {
                    user.InternalAddClients(this);
                }
            }
        }
        #endregion
        #region Constructors
/// <summary>
/// Public constructors of the ApplicationClient class
/// </summary>
/// <returns>New ApplicationClient object</returns>
/// <remarks></remarks>
        public ApplicationClient() {}
        #endregion
        #region Methods

        public virtual List<string> _Validate(bool throwException = true)
        {
            var __errors = new List<string>();
            if (Id == null)
            {
                __errors.Add("Property 'Id' is required.");
            }
            if (ClientKey != null && ClientKey.Length > 500)
            {
                __errors.Add("Length of property 'ClientKey' cannot be greater than 500.");
            }
            if (IPAddress != null && IPAddress.Length > 100)
            {
                __errors.Add("Length of property 'IPAddress' cannot be greater than 100.");
            }
            if (SessionId != null && SessionId.Length > 100)
            {
                __errors.Add("Length of property 'SessionId' cannot be greater than 100.");
            }
            if (User == null)
            {
                __errors.Add("Association with 'User' is required.");
            }
            if (throwException && __errors.Any())
            {
                throw new CLMS.Framework.Exceptions.BusinessException("An instance of TypeClass 'ApplicationClient' has validation errors:\r\n\r\n" + string.Join("\r\n", __errors));
            }
            return __errors;
        }

        public virtual int _GetUniqueIdentifier()
        {
            var hashCode = 399326290;
            hashCode = hashCode * -1521134295 + (Id?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (ClientKey?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (IPAddress?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (SessionId?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (ConnectedOn?.GetHashCode() ?? 0);
            return hashCode;
        }






/// <summary>
/// Copies the current object to a new instance
/// </summary>
/// <param name="deep">Copy members that refer to objects external to this class (not dependent)</param>
/// <param name="copiedObjects">Objects that should be reused</param>
/// <param name="asNew">Copy the current object as a new one, ready to be persisted, along all its members.</param>
/// <param name="reuseNestedObjects">If asNew is true, this flag if set, forces the reuse of all external objects.</param>
/// <param name="copy">Optional - An existing [ApplicationClient] instance to use as the destination.</param>
/// <returns>A copy of the object</returns>
        public virtual ApplicationClient Copy(bool deep=false, Hashtable copiedObjects=null, bool asNew=false, bool reuseNestedObjects = false, ApplicationClient copy = null)
        {
            if(copiedObjects == null)
            {
                copiedObjects = new Hashtable();
            }
            if (copy == null && copiedObjects.Contains(this))
                return (ApplicationClient)copiedObjects[this];
            copy = copy ?? new ApplicationClient();
            if (!asNew)
            {
                copy.TransientId = this.TransientId;
                copy.Id = this.Id;
            }
            copy.ClientKey = this.ClientKey;
            copy.IPAddress = this.IPAddress;
            copy.SessionId = this.SessionId;
            copy.ConnectedOn = this.ConnectedOn;
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
            var compareTo = obj as ApplicationClient;
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
        public static bool operator ==(ApplicationClient x, ApplicationClient y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Equals(x, y);
        }

// Maintain inequality operator semantics for entities.
        public static bool operator !=(ApplicationClient x, ApplicationClient y)
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
        protected bool HasSameNonDefaultIdAs(ApplicationClient compareTo)
        {
            return !this.IsTransient() && !compareTo.IsTransient() && this.Id.Equals(compareTo.Id);
        }

        #endregion


    }
}
