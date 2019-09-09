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
    /// The ApplicationUserAction class
    ///
    /// </summary>
    [Serializable]
    [DataContract]
    public class ApplicationUserAction : IDomainModelClass
    {
        #region ApplicationUserAction's Fields

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
        [DataMember(Name="UserName")]
        protected string userName;
        [DataMember(Name="ActiveRoles")]
        protected string activeRoles;
        [DataMember(Name="ActivePermissions")]
        protected string activePermissions;
        [DataMember(Name="Action")]
        protected string action;
        [DataMember(Name="Controller")]
        protected string controller;
        [DataMember(Name="Date")]
        protected DateTime? date;
        [DataMember(Name="ErrorMessage")]
        protected string errorMessage;
        [DataMember(Name="Success")]
        protected bool success;
        #endregion
        #region ApplicationUserAction's Properties
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
/// The UserName property
///
/// </summary>
///
        public virtual string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                userName = value;
            }
        }
/// <summary>
/// The ActiveRoles property
///
/// </summary>
///
        public virtual string ActiveRoles
        {
            get
            {
                return activeRoles;
            }
            set
            {
                activeRoles = value;
            }
        }
/// <summary>
/// The ActivePermissions property
///
/// </summary>
///
        public virtual string ActivePermissions
        {
            get
            {
                return activePermissions;
            }
            set
            {
                activePermissions = value;
            }
        }
/// <summary>
/// The Action property
///
/// </summary>
///
        public virtual string Action
        {
            get
            {
                return action;
            }
            set
            {
                action = value;
            }
        }
/// <summary>
/// The Controller property
///
/// </summary>
///
        public virtual string Controller
        {
            get
            {
                return controller;
            }
            set
            {
                controller = value;
            }
        }
/// <summary>
/// The Date property
///
/// </summary>
///
        public virtual DateTime? Date
        {
            get
            {
                return date;
            }
            set
            {
                date = value;
            }
        }
/// <summary>
/// The ErrorMessage property
///
/// </summary>
///
        public virtual string ErrorMessage
        {
            get
            {
                return errorMessage;
            }
            set
            {
                errorMessage = value;
            }
        }
/// <summary>
/// The Success property
///
/// </summary>
///
        public virtual bool Success
        {
            get
            {
                return success;
            }
            set
            {
                success = value;
            }
        }
        #endregion
        #region Constructors
/// <summary>
/// Public constructors of the ApplicationUserAction class
/// </summary>
/// <returns>New ApplicationUserAction object</returns>
/// <remarks></remarks>
        public ApplicationUserAction() {}
        #endregion
        #region Methods

        public virtual List<string> _Validate(bool throwException = true)
        {
            var __errors = new List<string>();
            if (Id == null)
            {
                __errors.Add("Property 'Id' is required.");
            }
            if (UserName != null && UserName.Length > 500)
            {
                __errors.Add("Length of property 'UserName' cannot be greater than 500.");
            }
            if (ActiveRoles != null && ActiveRoles.Length > 4000)
            {
                __errors.Add("Length of property 'ActiveRoles' cannot be greater than 4000.");
            }
            if (ActivePermissions != null && ActivePermissions.Length > 4000)
            {
                __errors.Add("Length of property 'ActivePermissions' cannot be greater than 4000.");
            }
            if (Action != null && Action.Length > 500)
            {
                __errors.Add("Length of property 'Action' cannot be greater than 500.");
            }
            if (Controller != null && Controller.Length > 500)
            {
                __errors.Add("Length of property 'Controller' cannot be greater than 500.");
            }
            if (ErrorMessage != null && ErrorMessage.Length > 4000)
            {
                __errors.Add("Length of property 'ErrorMessage' cannot be greater than 4000.");
            }
            if (throwException && __errors.Any())
            {
                throw new BusinessException("An instance of TypeClass 'ApplicationUserAction' has validation errors:\r\n\r\n" + string.Join("\r\n", __errors));
            }
            return __errors;
        }

        public virtual int _GetUniqueIdentifier()
        {
            var hashCode = 399326290;
            hashCode = hashCode * -1521134295 + (Id?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (UserName?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (ActiveRoles?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (ActivePermissions?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Action?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Controller?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Date?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (ErrorMessage?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Success.GetHashCode() );
            return hashCode;
        }






/// <summary>
/// Copies the current object to a new instance
/// </summary>
/// <param name="deep">Copy members that refer to objects external to this class (not dependent)</param>
/// <param name="copiedObjects">Objects that should be reused</param>
/// <param name="asNew">Copy the current object as a new one, ready to be persisted, along all its members.</param>
/// <param name="reuseNestedObjects">If asNew is true, this flag if set, forces the reuse of all external objects.</param>
/// <param name="copy">Optional - An existing [ApplicationUserAction] instance to use as the destination.</param>
/// <returns>A copy of the object</returns>
        public virtual ApplicationUserAction Copy(bool deep=false, Hashtable copiedObjects=null, bool asNew=false, bool reuseNestedObjects = false, ApplicationUserAction copy = null)
        {
            if(copiedObjects == null)
            {
                copiedObjects = new Hashtable();
            }
            if (copy == null && copiedObjects.Contains(this))
                return (ApplicationUserAction)copiedObjects[this];
            copy = copy ?? new ApplicationUserAction();
            if (!asNew)
            {
                copy.TransientId = this.TransientId;
                copy.Id = this.Id;
            }
            copy.UserName = this.UserName;
            copy.ActiveRoles = this.ActiveRoles;
            copy.ActivePermissions = this.ActivePermissions;
            copy.Action = this.Action;
            copy.Controller = this.Controller;
            copy.Date = this.Date;
            copy.ErrorMessage = this.ErrorMessage;
            copy.Success = this.Success;
            if (!copiedObjects.Contains(this))
            {
                copiedObjects.Add(this, copy);
            }
            return copy;
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as ApplicationUserAction;
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
        public static bool operator ==(ApplicationUserAction x, ApplicationUserAction y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Equals(x, y);
        }

// Maintain inequality operator semantics for entities.
        public static bool operator !=(ApplicationUserAction x, ApplicationUserAction y)
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
        protected bool HasSameNonDefaultIdAs(ApplicationUserAction compareTo)
        {
            return !this.IsTransient() && !compareTo.IsTransient() && this.Id.Equals(compareTo.Id);
        }

        #endregion


    }
}
