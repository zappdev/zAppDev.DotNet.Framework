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
using CLMS.Framework.Exceptions;

namespace CLMS.Framework.Workflow
{
    /// <summary>
    /// The WorkflowExecutionResult class
    ///
    /// </summary>
    [Serializable]
    [DataContract]
    public class WorkflowExecutionResult : IWorkflowExecutionResult
    {
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
        [DataMember(Name = "Status")]
        protected WorkflowStatus? status;
        [DataMember(Name = "WorkflowExecutionResultKey")]
        protected int? workflowExecutionResultKey = 0;
     
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
        /// <summary>
        /// The WorkflowExecutionResultKey property
        ///
        /// </summary>
        ///
        [Key]
        public virtual int? WorkflowExecutionResultKey
        {
            get
            {
                return workflowExecutionResultKey;
            }
            set
            {
                workflowExecutionResultKey = value;
            }
        }

        [DataMember(Name = "Context")]
        protected WorkflowContextBase context;
        public virtual WorkflowContextBase Context
        {
            get
            {
                return context;
            }
            set
            {
                if (Equals(context, value)) return;
                var __oldValue = context;
                if (value != null)
                {
                    if (context != null && !Equals(context, value))
                        context.Result = null;
                    context = value;
                    if (context.Result != (IWorkflowExecutionResult)this)
                    {
                        context.Result = this;
                    }
                }
                else
                {
                    if (context != null)
                    {
                        var __obj = context;
                        context = null;
                        __obj.Result = null;
                    }
                }
            }
        }

        /// <summary>
        /// Public constructors of the WorkflowExecutionResult class
        /// </summary>
        /// <returns>New WorkflowExecutionResult object</returns>
        /// <remarks></remarks>
        public WorkflowExecutionResult() { }

        public virtual List<string> _Validate(bool throwException = true)
        {
            var __errors = new List<string>();
            if (Status == null)
            {
                __errors.Add("Property 'Status' is required.");
            }
            if (throwException && __errors.Any())
            {
                throw new BusinessException("An instance of TypeClass 'WorkflowExecutionResult' has validation errors:\r\n\r\n" + string.Join("\r\n", __errors));
            }
            return __errors;
        }

        public virtual int _GetUniqueIdentifier()
        {
            var hashCode = 399326290;
            hashCode = hashCode * -1521134295 + (Status?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (WorkflowExecutionResultKey?.GetHashCode() ?? 0);
            return hashCode;
        }

        /// <summary>
        /// Copies the current object to a new instance
        /// </summary>
        /// <param name="deep">Copy members that refer to objects external to this class (not dependent)</param>
        /// <param name="copiedObjects">Objects that should be reused</param>
        /// <param name="asNew">Copy the current object as a new one, ready to be persisted, along all its members.</param>
        /// <param name="reuseNestedObjects">If asNew is true, this flag if set, forces the reuse of all external objects.</param>
        /// <param name="copy">Optional - An existing [WorkflowExecutionResult] instance to use as the destination.</param>
        /// <returns>A copy of the object</returns>
        public virtual WorkflowExecutionResult Copy(bool deep = false, Hashtable copiedObjects = null, bool asNew = false, bool reuseNestedObjects = false, WorkflowExecutionResult copy = null)
        {
            if (copiedObjects == null)
            {
                copiedObjects = new Hashtable();
            }
            if (copy == null && copiedObjects.Contains(this))
                return (WorkflowExecutionResult)copiedObjects[this];
            copy = copy ?? new WorkflowExecutionResult();
            if (!asNew)
            {
                copy.TransientId = TransientId;
                copy.WorkflowExecutionResultKey = WorkflowExecutionResultKey;
            }
            copy.Status = Status;
            if (!copiedObjects.Contains(this))
            {
                copiedObjects.Add(this, copy);
            }
            if (deep && context != null)
            {
                if (!copiedObjects.Contains(context))
                {
                    if (asNew && reuseNestedObjects)
                        copy.Context = Context;
                    else if (asNew)
                        copy.Context = Context.Copy(deep, copiedObjects, true);
                    else
                        copy.context = context.Copy(deep, copiedObjects, false);
                }
                else
                {
                    if (asNew)
                        copy.Context = (WorkflowContextBase) copiedObjects[Context];
                    else
                        copy.context = (WorkflowContextBase) copiedObjects[Context];
                }
            }
            return copy;
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as WorkflowExecutionResult;
            if (ReferenceEquals(this, compareTo))
            {
                return true;
            }
            if (compareTo == null || !GetType().Equals(compareTo.GetTypeUnproxied()))
            {
                return false;
            }
            if (HasSameNonDefaultIdAs(compareTo))
            {
                return true;
            }
            // Since the Ids aren't the same, both of them must be transient to
            // compare domain signatures; because if one is transient and the
            // other is a persisted entity, then they cannot be the same object.
            return IsTransient() && compareTo.IsTransient() && (base.Equals(compareTo) || TransientId.Equals(compareTo.TransientId));
        }

        // Maintain equality operator semantics for entities.
        public static bool operator ==(WorkflowExecutionResult x, WorkflowExecutionResult y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Equals(x, y);
        }

        // Maintain inequality operator semantics for entities.
        public static bool operator !=(WorkflowExecutionResult x, WorkflowExecutionResult y)
        {
            return !(x == y);
        }

        private PropertyInfo __propertyKeyCache;
        public virtual PropertyInfo GetPrimaryKey()
        {
            if (__propertyKeyCache == null)
            {
                __propertyKeyCache = GetType().GetProperty("WorkflowExecutionResultKey");
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
            if (cachedHashcode.HasValue)
            {
                return cachedHashcode.Value;
            }
            if (IsTransient())
            {
                //this.cachedHashcode = base.GetHashCode();
                return TransientId.GetHashCode(); //don't cache because this won't stay transient forever
            }
            else
            {
                unchecked
                {
                    // It's possible for two objects to return the same hash code based on
                    // identically valued properties, even if they're of two different types,
                    // so we include the object's type in the hash calculation
                    var hashCode = GetType().GetHashCode();
                    cachedHashcode = (hashCode * HashMultiplier) ^ WorkflowExecutionResultKey.GetHashCode();
                }
            }
            return cachedHashcode.Value;
        }

        /// <summary>
        ///     Transient objects are not associated with an item already in storage.  For instance,
        ///     a Customer is transient if its Id is 0.  It's virtual to allow NHibernate-backed
        ///     objects to be lazily loaded.
        /// </summary>
        public virtual bool IsTransient()
        {
            return WorkflowExecutionResultKey == default(int) || WorkflowExecutionResultKey.Equals(default(int));
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
            return GetType();
        }

        /// <summary>
        ///     Returns true if self and the provided entity have the same Id values
        ///     and the Ids are not of the default Id value
        /// </summary>
        protected bool HasSameNonDefaultIdAs(WorkflowExecutionResult compareTo)
        {
            return !IsTransient() && !compareTo.IsTransient() && WorkflowExecutionResultKey.Equals(compareTo.WorkflowExecutionResultKey);
        }

    }
}
