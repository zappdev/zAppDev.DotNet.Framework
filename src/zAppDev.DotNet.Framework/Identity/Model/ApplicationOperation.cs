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
    /// The ApplicationOperation class
    ///
    /// </summary>
    [Serializable]
    [DataContract]
    public class ApplicationOperation : IDomainModelClass
    {
        #region ApplicationOperation's Fields

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
        [DataMember(Name="ParentControllerName")]
        protected string parentControllerName;
        [DataMember(Name="Type")]
        protected string type;
        [DataMember(Name="IsAvailableToAnonymous")]
        protected bool isAvailableToAnonymous;
        [DataMember(Name="IsAvailableToAllAuthorizedUsers")]
        protected bool isAvailableToAllAuthorizedUsers;

#pragma warning disable 0649
        private bool disableInternalAdditions;
#pragma warning restore 0649
        #endregion
        #region ApplicationOperation's Properties
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
/// The ParentControllerName property
///
/// </summary>
///
        public virtual string ParentControllerName
        {
            get
            {
                return parentControllerName;
            }
            set
            {
                parentControllerName = value;
            }
        }
/// <summary>
/// The Type property
///
/// </summary>
///
        public virtual string Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }
/// <summary>
/// The IsAvailableToAnonymous property
///
/// </summary>
///
        public virtual bool IsAvailableToAnonymous
        {
            get
            {
                return isAvailableToAnonymous;
            }
            set
            {
                isAvailableToAnonymous = value;
            }
        }
/// <summary>
/// The IsAvailableToAllAuthorizedUsers property
///
/// </summary>
///
        public virtual bool IsAvailableToAllAuthorizedUsers
        {
            get
            {
                return isAvailableToAllAuthorizedUsers;
            }
            set
            {
                isAvailableToAllAuthorizedUsers = value;
            }
        }

        #endregion
        #region ApplicationOperation's Participant Properties
        [DataMember(Name="Permissions")]
        protected IList<ApplicationPermission> permissions = new List<ApplicationPermission>();
        public virtual List<ApplicationPermission> Permissions
        {
            get
            {
                if (permissions is ApplicationPermission[])
                {
                    permissions = permissions.ToList();
                }
                if (permissions == null)
                {
                    permissions = new List<ApplicationPermission>();
                }
                return permissions.ToList();
            }
            set
            {
                if (permissions is ApplicationPermission[])
                {
                    permissions = permissions.ToList();
                }
                if (permissions != null)
                {
                    var __itemsToDelete = new List<ApplicationPermission>(permissions);
                    foreach (var __item in __itemsToDelete)
                    {
                        RemovePermissions(__item);
                    }
                }
                if(value == null)
                {
                    permissions = new List<ApplicationPermission>();
                    return;
                }
                foreach(var __item in value)
                {
                    AddPermissions(__item);
                }
            }
        }
        public virtual void AddPermissions(IList<ApplicationPermission> __items)
        {
            foreach (var __item in __items)
            {
                AddPermissions(__item);
            }
        }

        public virtual void InternalAddPermissions(ApplicationPermission __item)
        {
            if (__item == null || disableInternalAdditions) return;
            permissions?.Add(__item);
        }

        public virtual void InternalRemovePermissions(ApplicationPermission __item)
        {
            if (__item == null) return;
            permissions?.Remove(__item);
        }

        public virtual void AddPermissions(ApplicationPermission __item)
        {
            if (__item == null) return;
            if (!permissions.Contains(__item))
                InternalAddPermissions(__item);
            if (!__item.Operations.Contains(this))
                __item.AddOperations(this);
        }

        public virtual void AddAtIndexPermissions(int index, ApplicationPermission __item)
        {
            if (__item == null) return;
            if (!permissions.Contains(__item))
                permissions.Insert(index, __item);
            if (!__item.Operations.Contains(this))
                __item.AddOperations(this);
        }

        public virtual void RemovePermissions(ApplicationPermission __item)
        {
            if (__item != null)
            {
                if (permissions.Contains(__item))
                    InternalRemovePermissions(__item);
                if(__item.Operations.Contains(this))
                    __item.RemoveOperations(this);
            }
        }
        public virtual void SetPermissionsAt(ApplicationPermission __item, int __index)
        {
            if (__item == null)
            {
                permissions[__index].RemoveOperations(this);
            }
            else
            {
                permissions[__index] = __item;
                if (!__item.Operations.Contains(this))
                    __item.AddOperations(this);
            }
        }

        public virtual void ClearPermissions()
        {
            if (permissions!=null)
            {
                var __itemsToRemove = permissions.ToList();
                foreach(var __item in __itemsToRemove)
                {
                    RemovePermissions(__item);
                }
            }
        }
        #endregion
        #region Constructors
/// <summary>
/// Public constructors of the ApplicationOperation class
/// </summary>
/// <returns>New ApplicationOperation object</returns>
/// <remarks></remarks>
        public ApplicationOperation() {}
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
            if (ParentControllerName != null && ParentControllerName.Length > 100)
            {
                __errors.Add("Length of property 'ParentControllerName' cannot be greater than 100.");
            }
            if (Type != null && Type.Length > 100)
            {
                __errors.Add("Length of property 'Type' cannot be greater than 100.");
            }
            if (throwException && __errors.Any())
            {
                throw new BusinessException("An instance of TypeClass 'ApplicationOperation' has validation errors:\r\n\r\n" + string.Join("\r\n", __errors));
            }
            return __errors;
        }

        public virtual int _GetUniqueIdentifier()
        {
            var hashCode = 399326290;
            hashCode = hashCode * -1521134295 + (Id?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Name?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (ParentControllerName?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Type?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (IsAvailableToAnonymous.GetHashCode() );
            hashCode = hashCode * -1521134295 + (IsAvailableToAllAuthorizedUsers.GetHashCode() );
            return hashCode;
        }






/// <summary>
/// Copies the current object to a new instance
/// </summary>
/// <param name="deep">Copy members that refer to objects external to this class (not dependent)</param>
/// <param name="copiedObjects">Objects that should be reused</param>
/// <param name="asNew">Copy the current object as a new one, ready to be persisted, along all its members.</param>
/// <param name="reuseNestedObjects">If asNew is true, this flag if set, forces the reuse of all external objects.</param>
/// <param name="copy">Optional - An existing [ApplicationOperation] instance to use as the destination.</param>
/// <returns>A copy of the object</returns>
        public virtual ApplicationOperation Copy(bool deep=false, Hashtable copiedObjects=null, bool asNew=false, bool reuseNestedObjects = false, ApplicationOperation copy = null)
        {
            if(copiedObjects == null)
            {
                copiedObjects = new Hashtable();
            }
            if (copy == null && copiedObjects.Contains(this))
                return (ApplicationOperation)copiedObjects[this];
            copy = copy ?? new ApplicationOperation();
            if (!asNew)
            {
                copy.TransientId = this.TransientId;
                copy.Id = this.Id;
            }
            copy.Name = this.Name;
            copy.ParentControllerName = this.ParentControllerName;
            copy.Type = this.Type;
            copy.IsAvailableToAnonymous = this.IsAvailableToAnonymous;
            copy.IsAvailableToAllAuthorizedUsers = this.IsAvailableToAllAuthorizedUsers;
            if (!copiedObjects.Contains(this))
            {
                copiedObjects.Add(this, copy);
            }
            copy.permissions = new List<ApplicationPermission>();
            if(deep && this.permissions != null)
            {
                foreach (var __item in this.permissions)
                {
                    if (!copiedObjects.Contains(__item))
                    {
                        if (asNew && reuseNestedObjects)
                            copy.AddPermissions(__item);
                        else
                            copy.AddPermissions(__item.Copy(deep, copiedObjects, asNew));
                    }
                    else
                    {
                        copy.AddPermissions((ApplicationPermission)copiedObjects[__item]);
                    }
                }
            }
            return copy;
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as ApplicationOperation;
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
        public static bool operator ==(ApplicationOperation x, ApplicationOperation y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Equals(x, y);
        }

// Maintain inequality operator semantics for entities.
        public static bool operator !=(ApplicationOperation x, ApplicationOperation y)
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
        protected bool HasSameNonDefaultIdAs(ApplicationOperation compareTo)
        {
            return !this.IsTransient() && !compareTo.IsTransient() && this.Id.Equals(compareTo.Id);
        }

        #endregion


    }
}
