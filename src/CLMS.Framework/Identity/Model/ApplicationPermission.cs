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
    /// The ApplicationPermission class
    ///
    /// </summary>
    [Serializable]
    [DataContract]
    public class ApplicationPermission : IDomainModelClass
    {
        #region ApplicationPermission's Fields

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
        [DataMember(Name="Name")]
        protected string name;
        [DataMember(Name="Description")]
        protected string description;
        [DataMember(Name="IsCustom")]
        protected bool isCustom;
        [DataMember(Name="VersionTimestamp")]
        protected byte[] versionTimestamp;

#pragma warning disable 0649
        private bool disableInternalAdditions;
#pragma warning restore 0649
        #endregion
        #region ApplicationPermission's Properties
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
/// The Name property
///
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
/// The Description property
///
/// </summary>
///
        public virtual string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }
/// <summary>
/// The IsCustom property
///
/// </summary>
///
        public virtual bool IsCustom
        {
            get
            {
                return isCustom;
            }
            set
            {
                isCustom = value;
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
        #region ApplicationPermission's Participant Properties
        [DataMember(Name="Users")]
        protected IList<ApplicationUser> users = new List<ApplicationUser>();
        public virtual List<ApplicationUser> Users
        {
            get
            {
                if (users is ApplicationUser[])
                {
                    users = users.ToList();
                }
                if (users == null)
                {
                    users = new List<ApplicationUser>();
                }
                return users.ToList();
            }
            set
            {
                if (users is ApplicationUser[])
                {
                    users = users.ToList();
                }
                if (users != null)
                {
                    var __itemsToDelete = new List<ApplicationUser>(users);
                    foreach (var __item in __itemsToDelete)
                    {
                        RemoveUsers(__item);
                    }
                }
                if(value == null)
                {
                    users = new List<ApplicationUser>();
                    return;
                }
                foreach(var __item in value)
                {
                    AddUsers(__item);
                }
            }
        }
        public virtual void AddUsers(IList<ApplicationUser> __items)
        {
            foreach (var __item in __items)
            {
                AddUsers(__item);
            }
        }

        public virtual void InternalAddUsers(ApplicationUser __item)
        {
            if (__item == null || disableInternalAdditions) return;
            users?.Add(__item);
        }

        public virtual void InternalRemoveUsers(ApplicationUser __item)
        {
            if (__item == null) return;
            users?.Remove(__item);
        }

        public virtual void AddUsers(ApplicationUser __item)
        {
            if (__item == null) return;
            if (!users.Contains(__item))
                InternalAddUsers(__item);
            if (!__item.Permissions.Contains(this))
                __item.AddPermissions(this);
        }

        public virtual void AddAtIndexUsers(int index, ApplicationUser __item)
        {
            if (__item == null) return;
            if (!users.Contains(__item))
                users.Insert(index, __item);
            if (!__item.Permissions.Contains(this))
                __item.AddPermissions(this);
        }

        public virtual void RemoveUsers(ApplicationUser __item)
        {
            if (__item != null)
            {
                if (users.Contains(__item))
                    InternalRemoveUsers(__item);
                if(__item.Permissions.Contains(this))
                    __item.RemovePermissions(this);
            }
        }
        public virtual void SetUsersAt(ApplicationUser __item, int __index)
        {
            if (__item == null)
            {
                users[__index].RemovePermissions(this);
            }
            else
            {
                users[__index] = __item;
                if (!__item.Permissions.Contains(this))
                    __item.AddPermissions(this);
            }
        }

        public virtual void ClearUsers()
        {
            if (users!=null)
            {
                var __itemsToRemove = users.ToList();
                foreach(var __item in __itemsToRemove)
                {
                    RemoveUsers(__item);
                }
            }
        }
        [DataMember(Name="Roles")]
        protected IList<ApplicationRole> roles = new List<ApplicationRole>();
        public virtual List<ApplicationRole> Roles
        {
            get
            {
                if (roles is ApplicationRole[])
                {
                    roles = roles.ToList();
                }
                if (roles == null)
                {
                    roles = new List<ApplicationRole>();
                }
                return roles.ToList();
            }
            set
            {
                if (roles is ApplicationRole[])
                {
                    roles = roles.ToList();
                }
                if (roles != null)
                {
                    var __itemsToDelete = new List<ApplicationRole>(roles);
                    foreach (var __item in __itemsToDelete)
                    {
                        RemoveRoles(__item);
                    }
                }
                if(value == null)
                {
                    roles = new List<ApplicationRole>();
                    return;
                }
                foreach(var __item in value)
                {
                    AddRoles(__item);
                }
            }
        }
        public virtual void AddRoles(IList<ApplicationRole> __items)
        {
            foreach (var __item in __items)
            {
                AddRoles(__item);
            }
        }

        public virtual void InternalAddRoles(ApplicationRole __item)
        {
            if (__item == null || disableInternalAdditions) return;
            roles?.Add(__item);
        }

        public virtual void InternalRemoveRoles(ApplicationRole __item)
        {
            if (__item == null) return;
            roles?.Remove(__item);
        }

        public virtual void AddRoles(ApplicationRole __item)
        {
            if (__item == null) return;
            if (!roles.Contains(__item))
                InternalAddRoles(__item);
            if (!__item.Permissions.Contains(this))
                __item.AddPermissions(this);
        }

        public virtual void AddAtIndexRoles(int index, ApplicationRole __item)
        {
            if (__item == null) return;
            if (!roles.Contains(__item))
                roles.Insert(index, __item);
            if (!__item.Permissions.Contains(this))
                __item.AddPermissions(this);
        }

        public virtual void RemoveRoles(ApplicationRole __item)
        {
            if (__item != null)
            {
                if (roles.Contains(__item))
                    InternalRemoveRoles(__item);
                if(__item.Permissions.Contains(this))
                    __item.RemovePermissions(this);
            }
        }
        public virtual void SetRolesAt(ApplicationRole __item, int __index)
        {
            if (__item == null)
            {
                roles[__index].RemovePermissions(this);
            }
            else
            {
                roles[__index] = __item;
                if (!__item.Permissions.Contains(this))
                    __item.AddPermissions(this);
            }
        }

        public virtual void ClearRoles()
        {
            if (roles!=null)
            {
                var __itemsToRemove = roles.ToList();
                foreach(var __item in __itemsToRemove)
                {
                    RemoveRoles(__item);
                }
            }
        }
        [DataMember(Name="Operations")]
        protected IList<ApplicationOperation> operations = new List<ApplicationOperation>();
        public virtual List<ApplicationOperation> Operations
        {
            get
            {
                if (operations is ApplicationOperation[])
                {
                    operations = operations.ToList();
                }
                if (operations == null)
                {
                    operations = new List<ApplicationOperation>();
                }
                return operations.ToList();
            }
            set
            {
                if (operations is ApplicationOperation[])
                {
                    operations = operations.ToList();
                }
                if (operations != null)
                {
                    var __itemsToDelete = new List<ApplicationOperation>(operations);
                    foreach (var __item in __itemsToDelete)
                    {
                        RemoveOperations(__item);
                    }
                }
                if(value == null)
                {
                    operations = new List<ApplicationOperation>();
                    return;
                }
                foreach(var __item in value)
                {
                    AddOperations(__item);
                }
            }
        }
        public virtual void AddOperations(IList<ApplicationOperation> __items)
        {
            foreach (var __item in __items)
            {
                AddOperations(__item);
            }
        }

        public virtual void InternalAddOperations(ApplicationOperation __item)
        {
            if (__item == null || disableInternalAdditions) return;
            operations?.Add(__item);
        }

        public virtual void InternalRemoveOperations(ApplicationOperation __item)
        {
            if (__item == null) return;
            operations?.Remove(__item);
        }

        public virtual void AddOperations(ApplicationOperation __item)
        {
            if (__item == null) return;
            if (!operations.Contains(__item))
                InternalAddOperations(__item);
            if (!__item.Permissions.Contains(this))
                __item.AddPermissions(this);
        }

        public virtual void AddAtIndexOperations(int index, ApplicationOperation __item)
        {
            if (__item == null) return;
            if (!operations.Contains(__item))
                operations.Insert(index, __item);
            if (!__item.Permissions.Contains(this))
                __item.AddPermissions(this);
        }

        public virtual void RemoveOperations(ApplicationOperation __item)
        {
            if (__item != null)
            {
                if (operations.Contains(__item))
                    InternalRemoveOperations(__item);
                if(__item.Permissions.Contains(this))
                    __item.RemovePermissions(this);
            }
        }
        public virtual void SetOperationsAt(ApplicationOperation __item, int __index)
        {
            if (__item == null)
            {
                operations[__index].RemovePermissions(this);
            }
            else
            {
                operations[__index] = __item;
                if (!__item.Permissions.Contains(this))
                    __item.AddPermissions(this);
            }
        }

        public virtual void ClearOperations()
        {
            if (operations!=null)
            {
                var __itemsToRemove = operations.ToList();
                foreach(var __item in __itemsToRemove)
                {
                    RemoveOperations(__item);
                }
            }
        }
        #endregion
        #region Constructors
/// <summary>
/// Public constructors of the ApplicationPermission class
/// </summary>
/// <returns>New ApplicationPermission object</returns>
/// <remarks></remarks>
        public ApplicationPermission() {}
        #endregion
        #region Methods

        public virtual List<string> _Validate(bool throwException = true)
        {
            var __errors = new List<string>();
            if (Id == null)
            {
                __errors.Add("Property 'Id' is required.");
            }
            if (Name == null)
            {
                __errors.Add("Property 'Name' is required.");
            }
            if (Name != null && string.IsNullOrWhiteSpace(Name))
            {
                __errors.Add("String 'Name' cannot be empty.");
            }
            if (Name != null && Name.Length > 255)
            {
                __errors.Add("Length of property 'Name' cannot be greater than 255.");
            }
            if (Description != null && Description.Length > 1000)
            {
                __errors.Add("Length of property 'Description' cannot be greater than 1000.");
            }
            if (throwException && __errors.Any())
            {
                throw new CLMS.Framework.Exceptions.BusinessException("An instance of TypeClass 'ApplicationPermission' has validation errors:\r\n\r\n" + string.Join("\r\n", __errors));
            }
            return __errors;
        }

        public virtual int _GetUniqueIdentifier()
        {
            var hashCode = 399326290;
            hashCode = hashCode * -1521134295 + (Id?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Name?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Description?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (IsCustom.GetHashCode() );
            return hashCode;
        }






/// <summary>
/// Copies the current object to a new instance
/// </summary>
/// <param name="deep">Copy members that refer to objects external to this class (not dependent)</param>
/// <param name="copiedObjects">Objects that should be reused</param>
/// <param name="asNew">Copy the current object as a new one, ready to be persisted, along all its members.</param>
/// <param name="reuseNestedObjects">If asNew is true, this flag if set, forces the reuse of all external objects.</param>
/// <param name="copy">Optional - An existing [ApplicationPermission] instance to use as the destination.</param>
/// <returns>A copy of the object</returns>
        public virtual ApplicationPermission Copy(bool deep=false, Hashtable copiedObjects=null, bool asNew=false, bool reuseNestedObjects = false, ApplicationPermission copy = null)
        {
            if(copiedObjects == null)
            {
                copiedObjects = new Hashtable();
            }
            if (copy == null && copiedObjects.Contains(this))
                return (ApplicationPermission)copiedObjects[this];
            copy = copy ?? new ApplicationPermission();
            if (!asNew)
            {
                copy.TransientId = this.TransientId;
                copy.Id = this.Id;
            }
            copy.Name = this.Name;
            copy.Description = this.Description;
            copy.IsCustom = this.IsCustom;
            if (!copiedObjects.Contains(this))
            {
                copiedObjects.Add(this, copy);
            }
            copy.users = new List<ApplicationUser>();
            if(deep && this.users != null)
            {
                foreach (var __item in this.users)
                {
                    if (!copiedObjects.Contains(__item))
                    {
                        if (asNew && reuseNestedObjects)
                            copy.AddUsers(__item);
                        else
                            copy.AddUsers(__item.Copy(deep, copiedObjects, asNew));
                    }
                    else
                    {
                        copy.AddUsers((ApplicationUser)copiedObjects[__item]);
                    }
                }
            }
            copy.roles = new List<ApplicationRole>();
            if(deep && this.roles != null)
            {
                foreach (var __item in this.roles)
                {
                    if (!copiedObjects.Contains(__item))
                    {
                        if (asNew && reuseNestedObjects)
                            copy.AddRoles(__item);
                        else
                            copy.AddRoles(__item.Copy(deep, copiedObjects, asNew));
                    }
                    else
                    {
                        copy.AddRoles((ApplicationRole)copiedObjects[__item]);
                    }
                }
            }
            copy.operations = new List<ApplicationOperation>();
            if(deep && this.operations != null)
            {
                foreach (var __item in this.operations)
                {
                    if (!copiedObjects.Contains(__item))
                    {
                        if (asNew && reuseNestedObjects)
                            copy.AddOperations(__item);
                        else
                            copy.AddOperations(__item.Copy(deep, copiedObjects, asNew));
                    }
                    else
                    {
                        copy.AddOperations((ApplicationOperation)copiedObjects[__item]);
                    }
                }
            }
            return copy;
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as ApplicationPermission;
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
        public static bool operator ==(ApplicationPermission x, ApplicationPermission y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Equals(x, y);
        }

// Maintain inequality operator semantics for entities.
        public static bool operator !=(ApplicationPermission x, ApplicationPermission y)
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
        protected bool HasSameNonDefaultIdAs(ApplicationPermission compareTo)
        {
            return !this.IsTransient() && !compareTo.IsTransient() && this.Id.Equals(compareTo.Id);
        }

        #endregion


    }
}
