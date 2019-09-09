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

namespace zAppDev.DotNet.Framework.Workflow
{
    /// <summary>
    /// The WorkflowContextBase class
    ///
    /// </summary>
    [Serializable]
    [DataContract]
    public class WorkflowContextBase : IDomainModelClass, IWorkflowContext
    {
        #region WorkflowContextBase's Fields

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
        [DataMember(Name = "Name")]
        protected string name;
        [DataMember(Name = "Error")]
        protected string error;
        [DataMember(Name = "Expires")]
        protected bool expires;
        [DataMember(Name = "ExpirationDateTime")]
        protected DateTime? expirationDateTime;
        [DataMember(Name = "PendingSince")]
        protected DateTime? pendingSince;
        [DataMember(Name = "PendingJobCreatedBy")]
        protected string pendingJobCreatedBy;
        [DataMember(Name = "PendingStep")]
        protected string pendingStep;
        [DataMember(Name = "Id")]
        protected Guid? id = Guid.Empty;
        [DataMember(Name = "Status")]
        protected WorkflowStatus? status;
        #endregion
        #region WorkflowContextBase's Properties
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
        /// The Error property
        ///
        /// </summary>
        ///
        public virtual string Error
        {
            get
            {
                return error;
            }
            set
            {
                error = value;
            }
        }
        /// <summary>
        /// The Expires property
        ///
        /// </summary>
        ///
        public virtual bool Expires
        {
            get
            {
                return expires;
            }
            set
            {
                expires = value;
            }
        }
        /// <summary>
        /// The ExpirationDateTime property
        ///
        /// </summary>
        ///
        public virtual DateTime? ExpirationDateTime
        {
            get
            {
                return expirationDateTime;
            }
            set
            {
                expirationDateTime = value;
            }
        }
        /// <summary>
        /// The PendingSince property
        ///
        /// </summary>
        ///
        public virtual DateTime? PendingSince
        {
            get
            {
                return pendingSince;
            }
            set
            {
                pendingSince = value;
            }
        }
        /// <summary>
        /// The PendingJobCreatedBy property
        ///
        /// </summary>
        ///
        public virtual string PendingJobCreatedBy
        {
            get
            {
                return pendingJobCreatedBy;
            }
            set
            {
                pendingJobCreatedBy = value;
            }
        }
        /// <summary>
        /// The PendingStep property
        ///
        /// </summary>
        ///
        public virtual string PendingStep
        {
            get
            {
                return pendingStep;
            }
            set
            {
                pendingStep = value;
            }
        }
        /// <summary>
        /// The Id property
        ///Pending Job Key
        /// </summary>
        ///
        [Key]
        public virtual Guid? Id
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
        /// The Status property
        ///
        /// </summary>
        ///
        public virtual WorkflowStatus? Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
            }
        }
        #endregion
        #region WorkflowContextBase's Participant Properties
        [DataMember(Name = "Result")]
        protected WorkflowExecutionResult result;
        public virtual WorkflowExecutionResult Result
        {
            get
            {
                return result;
            }
            set
            {
                if (Equals(result, value)) return;
                var __oldValue = result;
                if (value != null)
                {
                    if (result != null && !Equals(result, value))
                        result.Context = null;
                    result = value;
                    if (result.Context != this)
                        result.Context = this;
                }
                else
                {
                    if (result != null)
                    {
                        var __obj = result;
                        result = null;
                        __obj.Context = null;
                    }
                }
            }
        }
        #endregion
        #region Constructors
        /// <summary>
        /// Public constructors of the WorkflowContextBase class
        /// </summary>
        /// <returns>New WorkflowContextBase object</returns>
        /// <remarks></remarks>
        public WorkflowContextBase() { }
        #endregion
        #region Methods

        public virtual List<string> _Validate(bool throwException = true)
        {
            var __errors = new List<string>();
            if (Name != null && Name.Length > 512)
            {
                __errors.Add("Length of property 'Name' cannot be greater than 512.");
            }
            if (Error != null && Error.Length > 4000)
            {
                __errors.Add("Length of property 'Error' cannot be greater than 4000.");
            }
            if (PendingJobCreatedBy != null && PendingJobCreatedBy.Length > 512)
            {
                __errors.Add("Length of property 'PendingJobCreatedBy' cannot be greater than 512.");
            }
            if (PendingStep != null && PendingStep.Length > 512)
            {
                __errors.Add("Length of property 'PendingStep' cannot be greater than 512.");
            }
            if (Id == null)
            {
                __errors.Add("Property 'Id' is required.");
            }
            if (Status == null)
            {
                __errors.Add("Property 'Status' is required.");
            }
            if (throwException && __errors.Any())
            {
                throw new BusinessException("An instance of TypeClass 'WorkflowContextBase' has validation errors:\r\n\r\n" + string.Join("\r\n", __errors));
            }
            return __errors;
        }

        public virtual int _GetUniqueIdentifier()
        {
            var hashCode = 399326290;
            hashCode = hashCode * -1521134295 + (Name?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Error?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Expires.GetHashCode());
            hashCode = hashCode * -1521134295 + (ExpirationDateTime?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (PendingSince?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (PendingJobCreatedBy?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (PendingStep?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Id?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Status?.GetHashCode() ?? 0);
            return hashCode;
        }

        /// <summary>
        /// Copies the current object to a new instance
        /// </summary>
        /// <param name="deep">Copy members that refer to objects external to this class (not dependent)</param>
        /// <param name="copiedObjects">Objects that should be reused</param>
        /// <param name="asNew">Copy the current object as a new one, ready to be persisted, along all its members.</param>
        /// <param name="reuseNestedObjects">If asNew is true, this flag if set, forces the reuse of all external objects.</param>
        /// <param name="copy">Optional - An existing [WorkflowContextBase] instance to use as the destination.</param>
        /// <returns>A copy of the object</returns>
        public virtual WorkflowContextBase Copy(bool deep = false, Hashtable copiedObjects = null, bool asNew = false, bool reuseNestedObjects = false, WorkflowContextBase copy = null)
        {
            if (copiedObjects == null)
            {
                copiedObjects = new Hashtable();
            }
            if (copy == null && copiedObjects.Contains(this))
                return (WorkflowContextBase)copiedObjects[this];
            copy = copy ?? new WorkflowContextBase();
            if (!asNew)
            {
                copy.TransientId = this.TransientId;
                copy.Id = this.Id;
            }
            copy.Name = this.Name;
            copy.Error = this.Error;
            copy.Expires = this.Expires;
            copy.ExpirationDateTime = this.ExpirationDateTime;
            copy.PendingSince = this.PendingSince;
            copy.PendingJobCreatedBy = this.PendingJobCreatedBy;
            copy.PendingStep = this.PendingStep;
            copy.Status = this.Status;
            if (!copiedObjects.Contains(this))
            {
                copiedObjects.Add(this, copy);
            }
            if (deep && this.result != null)
            {
                if (!copiedObjects.Contains(this.result))
                {
                    if (asNew && reuseNestedObjects)
                        copy.Result = this.Result;
                    else if (asNew)
                        copy.Result = this.Result.Copy(deep, copiedObjects, true);
                    else
                        copy.result = this.result.Copy(deep, copiedObjects, false);
                }
                else
                {
                    if (asNew)
                        copy.Result = (WorkflowExecutionResult)copiedObjects[this.Result];
                    else
                        copy.result = (WorkflowExecutionResult)copiedObjects[this.Result];
                }
            }
            return copy;
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as WorkflowContextBase;
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
        public static bool operator ==(WorkflowContextBase x, WorkflowContextBase y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Equals(x, y);
        }

        // Maintain inequality operator semantics for entities.
        public static bool operator !=(WorkflowContextBase x, WorkflowContextBase y)
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
            return this.Id == default(Guid) || this.Id.Equals(default(Guid));
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
        protected bool HasSameNonDefaultIdAs(WorkflowContextBase compareTo)
        {
            return !this.IsTransient() && !compareTo.IsTransient() && this.Id.Equals(compareTo.Id);
        }

        #endregion

        public virtual IWorkflowExecutionResult Continue()
        {
            return WorkflowManager.Current.Continue(this?.Id ?? default);
        }

        public virtual IWorkflowExecutionResult Cancel()
        {
            return WorkflowManager.Current.Cancel(this?.Id ?? default);
        }

        public virtual IWorkflowExecutionResult Expire()
        {
            return WorkflowManager.Current.Expire(this?.Id ?? default);
        }
    }
}
