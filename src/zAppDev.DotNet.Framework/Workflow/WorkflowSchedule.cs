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
using System.Data.SqlTypes;
using NCrontab;
using zAppDev.DotNet.Framework.Exceptions;

namespace zAppDev.DotNet.Framework.Workflow
{
    /// <summary>
    /// The WorkflowSchedule class
    /// Workflow Schedule
    /// </summary>
    [Serializable]
    [DataContract]
    public class WorkflowSchedule : IDomainModelClass
    {
        #region WorkflowSchedule's Fields

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
        [DataMember(Name = "Workflow")]
        protected string workflow = string.Empty;
        [DataMember(Name = "Description")]
        protected string description;
        [DataMember(Name = "StartDateTime")]
        protected DateTime? startDateTime;
        [DataMember(Name = "ExpireOn")]
        protected DateTime? expireOn;
        [DataMember(Name = "CronExpression")]
        protected string cronExpression;
        [DataMember(Name = "LastExecution")]
        protected DateTime? lastExecution;
        [DataMember(Name = "LastExecutionMessage")]
        protected string lastExecutionMessage;
        [DataMember(Name = "IsLastExecutionSuccess")]
        protected bool isLastExecutionSuccess;
        [DataMember(Name = "Active")]
        protected bool active;
        [DataMember(Name = "HumanReadableExpression")]
        protected string humanReadableExpression;
        [DataMember(Name = "NextExecutionTime")]
        protected DateTime? nextExecutionTime;
        #endregion
        #region WorkflowSchedule's Properties
        /// <summary>
        /// The Workflow property
        ///
        /// </summary>
        ///
        [Key]
        public virtual string Workflow
        {
            get
            {
                return workflow;
            }
            set
            {
                workflow = value;
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
        /// The StartDateTime property
        ///
        /// </summary>
        ///
        public virtual DateTime? StartDateTime
        {
            get
            {
                return startDateTime;
            }
            set
            {
                startDateTime = value;
            }
        }
        /// <summary>
        /// The ExpireOn property
        ///
        /// </summary>
        ///
        public virtual DateTime? ExpireOn
        {
            get
            {
                return expireOn;
            }
            set
            {
                expireOn = value;
            }
        }
        /// <summary>
        /// The CronExpression property
        ///
        /// </summary>
        ///
        public virtual string CronExpression
        {
            get
            {
                return cronExpression;
            }
            set
            {
                cronExpression = value;
            }
        }
        /// <summary>
        /// The LastExecution property
        ///
        /// </summary>
        ///
        public virtual DateTime? LastExecution
        {
            get
            {
                return lastExecution;
            }
            set
            {
                lastExecution = value;
            }
        }
        /// <summary>
        /// The LastExecutionMessage property
        ///
        /// </summary>
        ///
        public virtual string LastExecutionMessage
        {
            get
            {
                return lastExecutionMessage;
            }
            set
            {
                lastExecutionMessage = value;
            }
        }
        /// <summary>
        /// The IsLastExecutionSuccess property
        ///
        /// </summary>
        ///
        public virtual bool IsLastExecutionSuccess
        {
            get
            {
                return isLastExecutionSuccess;
            }
            set
            {
                isLastExecutionSuccess = value;
            }
        }
        /// <summary>
        /// The Active property
        ///
        /// </summary>
        ///
        public virtual bool Active
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
            }
        }
        /// <summary>
        /// The HumanReadableExpression property
        ///
        /// </summary>
        ///
        public virtual string HumanReadableExpression
        {
            get
            {
                var __valToGet = GetHumanReadableExpressionImplementation();
                return __valToGet;
            }
            set
            {
                humanReadableExpression = value;
            }
        }
        /// <summary>
        /// The NextExecutionTime property
        ///
        /// </summary>
        ///
        public virtual DateTime? NextExecutionTime
        {
            get
            {
                var __valToGet = GetNextExecutionTimeImplementation();
                return __valToGet;
            }
            set
            {
                nextExecutionTime = value;
            }
        }
        #endregion
        #region Constructors
        /// <summary>
        /// Public constructors of the WorkflowSchedule class
        /// </summary>
        /// <returns>New WorkflowSchedule object</returns>
        /// <remarks></remarks>
        public WorkflowSchedule() { }
        #endregion
        #region Accessors Implementation
        private string GetHumanReadableExpressionImplementation()
        {
            if (CrontabSchedule.TryParse((this?.cronExpression ?? "")) == null == false)
            {
                return null;
            }
            return Utilities.CronExpressionDescriptor.ExpressionDescriptor.GetDescription((this?.cronExpression ?? ""));
        }
        private DateTime? GetNextExecutionTimeImplementation()
        {
            if ((((this?.active ?? false)) == false) || (this?.expireOn ?? System.Data.SqlTypes.SqlDateTime.MinValue.Value) >= DateTime.UtcNow || (((NCrontab.CrontabSchedule.TryParse((this?.cronExpression ?? "")) == null)) == false))
            {
                return null;
            }
            if (this?.startDateTime == null || (this?.startDateTime ?? System.Data.SqlTypes.SqlDateTime.MinValue.Value) <= DateTime.UtcNow)
            {
                return Utilities.Common.GetNextExecutionTime((this?.cronExpression ?? ""), DateTime.UtcNow);
            }
            return Utilities.Common.GetNextExecutionTime((this?.cronExpression ?? ""), (this?.startDateTime ?? System.Data.SqlTypes.SqlDateTime.MinValue.Value));
        }
        #endregion
        #region Methods

        public virtual List<string> _Validate(bool throwException = true)
        {
            var __errors = new List<string>();
            if (Workflow == null)
            {
                __errors.Add("Property 'Workflow' is required.");
            }
            if (Workflow != null && string.IsNullOrWhiteSpace(Workflow))
            {
                __errors.Add("String 'Workflow' cannot be empty.");
            }
            if (Workflow != null && Workflow.Length > 255)
            {
                __errors.Add("Length of property 'Workflow' cannot be greater than 255.");
            }
            if (Description != null && Description.Length > 1000)
            {
                __errors.Add("Length of property 'Description' cannot be greater than 1000.");
            }
            if (CronExpression != null && CronExpression.Length > 100)
            {
                __errors.Add("Length of property 'CronExpression' cannot be greater than 100.");
            }
            if (LastExecutionMessage != null && LastExecutionMessage.Length > 2147483647)
            {
                __errors.Add("Length of property 'LastExecutionMessage' cannot be greater than 2147483647.");
            }
            if (HumanReadableExpression != null && HumanReadableExpression.Length > 100)
            {
                __errors.Add("Length of property 'HumanReadableExpression' cannot be greater than 100.");
            }
            if (throwException && __errors.Any())
            {
                throw new BusinessException("An instance of TypeClass 'WorkflowSchedule' has validation errors:\r\n\r\n" + string.Join("\r\n", __errors));
            }
            return __errors;
        }

        public virtual int _GetUniqueIdentifier()
        {
            var hashCode = 399326290;
            hashCode = hashCode * -1521134295 + (Workflow?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Description?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (StartDateTime?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (ExpireOn?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (CronExpression?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (LastExecution?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (LastExecutionMessage?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (IsLastExecutionSuccess.GetHashCode());
            hashCode = hashCode * -1521134295 + (Active.GetHashCode());
            hashCode = hashCode * -1521134295 + (HumanReadableExpression?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (NextExecutionTime?.GetHashCode() ?? 0);
            return hashCode;
        }

        /// <summary>
        /// Copies the current object to a new instance
        /// </summary>
        /// <param name="deep">Copy members that refer to objects external to this class (not dependent)</param>
        /// <param name="copiedObjects">Objects that should be reused</param>
        /// <param name="asNew">Copy the current object as a new one, ready to be persisted, along all its members.</param>
        /// <param name="reuseNestedObjects">If asNew is true, this flag if set, forces the reuse of all external objects.</param>
        /// <param name="copy">Optional - An existing [WorkflowSchedule] instance to use as the destination.</param>
        /// <returns>A copy of the object</returns>
        public virtual WorkflowSchedule Copy(bool deep = false, Hashtable copiedObjects = null, bool asNew = false, bool reuseNestedObjects = false, WorkflowSchedule copy = null)
        {
            if (copiedObjects == null)
            {
                copiedObjects = new Hashtable();
            }
            if (copy == null && copiedObjects.Contains(this))
                return (WorkflowSchedule)copiedObjects[this];
            copy = copy ?? new WorkflowSchedule();
            if (!asNew)
            {
                copy.TransientId = this.TransientId;
                copy.Workflow = this.Workflow;
            }
            copy.Description = this.Description;
            copy.StartDateTime = this.StartDateTime;
            copy.ExpireOn = this.ExpireOn;
            copy.CronExpression = this.CronExpression;
            copy.LastExecution = this.LastExecution;
            copy.LastExecutionMessage = this.LastExecutionMessage;
            copy.IsLastExecutionSuccess = this.IsLastExecutionSuccess;
            copy.Active = this.Active;
            copy.HumanReadableExpression = this.HumanReadableExpression;
            copy.NextExecutionTime = this.NextExecutionTime;
            if (!copiedObjects.Contains(this))
            {
                copiedObjects.Add(this, copy);
            }
            return copy;
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as WorkflowSchedule;
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
        public static bool operator ==(WorkflowSchedule x, WorkflowSchedule y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Equals(x, y);
        }

        // Maintain inequality operator semantics for entities.
        public static bool operator !=(WorkflowSchedule x, WorkflowSchedule y)
        {
            return !(x == y);
        }

        private PropertyInfo __propertyKeyCache;
        public virtual PropertyInfo GetPrimaryKey()
        {
            if (__propertyKeyCache == null)
            {
                __propertyKeyCache = this.GetType().GetProperty("Workflow");
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
                    this.cachedHashcode = (hashCode * HashMultiplier) ^ this.Workflow.GetHashCode();
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
            return string.IsNullOrEmpty(this.Workflow);
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
        protected bool HasSameNonDefaultIdAs(WorkflowSchedule compareTo)
        {
            return !this.IsTransient() && !compareTo.IsTransient() && this.Workflow.Equals(compareTo.Workflow);
        }

        #endregion

        public virtual string GetHumanReadableExpression()
        {
            if (CrontabSchedule.TryParse(CronExpression ?? "") == null == false)
            {
                return null;
            }
            return Utilities.CronExpressionDescriptor.ExpressionDescriptor.GetDescription(CronExpression ?? "");
        }

        public virtual DateTime? GetNextExecutionTime()
        {
            if ((Active == false) || (ExpireOn ?? SqlDateTime.MinValue.Value) >= DateTime.UtcNow || (CrontabSchedule.TryParse(CronExpression ?? "") == null == false))
            {
                return null;
            }
            if (StartDateTime == null || (StartDateTime ?? SqlDateTime.MinValue.Value) <= DateTime.UtcNow)
            {
                return Utilities.Common.GetNextExecutionTime(CronExpression ?? "", DateTime.UtcNow);
            }
            return Utilities.Common.GetNextExecutionTime(CronExpression ?? "", (StartDateTime ?? SqlDateTime.MinValue.Value));
        }

        public virtual WorkflowExecutionResult Execute()
        {
            return new ScheduleManager().ExecuteSchedule(this) as WorkflowExecutionResult;
        }

    }
}
