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
    /// The DateTimeFormat class
    ///
    /// </summary>
    [Serializable]
    [DataContract]
    public class DateTimeFormat : IDomainModelClass
    {
        #region DateTimeFormat's Fields

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
        [DataMember(Name="LongDatePattern")]
        protected string longDatePattern;
        [DataMember(Name="LongTimePattern")]
        protected string longTimePattern;
        [DataMember(Name="MonthDayPattern")]
        protected string monthDayPattern;
        [DataMember(Name="RFC1123Pattern")]
        protected string rFC1123Pattern;
        [DataMember(Name="ShortDatePattern")]
        protected string shortDatePattern;
        [DataMember(Name="ShortTimePattern")]
        protected string shortTimePattern;
        [DataMember(Name="YearMonthPattern")]
        protected string yearMonthPattern;

        #endregion
        #region DateTimeFormat's Properties
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
/// The LongDatePattern property
///
/// </summary>
///
        public virtual string LongDatePattern
        {
            get
            {
                return longDatePattern;
            }
            set
            {
                longDatePattern = value;
            }
        }
/// <summary>
/// The LongTimePattern property
///
/// </summary>
///
        public virtual string LongTimePattern
        {
            get
            {
                return longTimePattern;
            }
            set
            {
                longTimePattern = value;
            }
        }
/// <summary>
/// The MonthDayPattern property
///
/// </summary>
///
        public virtual string MonthDayPattern
        {
            get
            {
                return monthDayPattern;
            }
            set
            {
                monthDayPattern = value;
            }
        }
/// <summary>
/// The RFC1123Pattern property
///
/// </summary>
///
        public virtual string RFC1123Pattern
        {
            get
            {
                return rFC1123Pattern;
            }
            set
            {
                rFC1123Pattern = value;
            }
        }
/// <summary>
/// The ShortDatePattern property
///
/// </summary>
///
        public virtual string ShortDatePattern
        {
            get
            {
                return shortDatePattern;
            }
            set
            {
                shortDatePattern = value;
            }
        }
/// <summary>
/// The ShortTimePattern property
///
/// </summary>
///
        public virtual string ShortTimePattern
        {
            get
            {
                return shortTimePattern;
            }
            set
            {
                shortTimePattern = value;
            }
        }
/// <summary>
/// The YearMonthPattern property
///
/// </summary>
///
        public virtual string YearMonthPattern
        {
            get
            {
                return yearMonthPattern;
            }
            set
            {
                yearMonthPattern = value;
            }
        }

        #endregion
        #region DateTimeFormat's Participant Properties
        [DataMember(Name="ApplicationLanguage")]
        protected ApplicationLanguage applicationLanguage;
        public virtual ApplicationLanguage ApplicationLanguage
        {
            get
            {
                return applicationLanguage;
            }
            set
            {
                if(Equals(applicationLanguage, value)) return;
                var __oldValue = applicationLanguage;
                if (value != null)
                {
                    if(applicationLanguage != null && !Equals(applicationLanguage, value))
                        applicationLanguage.DateTimeFormat = null;
                    applicationLanguage = value;
                    if(applicationLanguage.DateTimeFormat != this)
                        applicationLanguage.DateTimeFormat = this;
                }
                else
                {
                    if (applicationLanguage != null)
                    {
                        var __obj = applicationLanguage;
                        applicationLanguage = null;
                        __obj.DateTimeFormat = null;
                    }
                }
            }
        }
        #endregion
        #region Constructors
/// <summary>
/// Public constructors of the DateTimeFormat class
/// </summary>
/// <returns>New DateTimeFormat object</returns>
/// <remarks></remarks>
        public DateTimeFormat() {}
        #endregion
        #region Methods

        public virtual List<string> _Validate(bool throwException = true)
        {
            var __errors = new List<string>();
            if (Id == null)
            {
                __errors.Add("Property 'Id' is required.");
            }
            if (LongDatePattern != null && LongDatePattern.Length > 100)
            {
                __errors.Add("Length of property 'LongDatePattern' cannot be greater than 100.");
            }
            if (LongTimePattern != null && LongTimePattern.Length > 100)
            {
                __errors.Add("Length of property 'LongTimePattern' cannot be greater than 100.");
            }
            if (MonthDayPattern != null && MonthDayPattern.Length > 100)
            {
                __errors.Add("Length of property 'MonthDayPattern' cannot be greater than 100.");
            }
            if (RFC1123Pattern != null && RFC1123Pattern.Length > 100)
            {
                __errors.Add("Length of property 'RFC1123Pattern' cannot be greater than 100.");
            }
            if (ShortDatePattern != null && ShortDatePattern.Length > 100)
            {
                __errors.Add("Length of property 'ShortDatePattern' cannot be greater than 100.");
            }
            if (ShortTimePattern != null && ShortTimePattern.Length > 100)
            {
                __errors.Add("Length of property 'ShortTimePattern' cannot be greater than 100.");
            }
            if (YearMonthPattern != null && YearMonthPattern.Length > 100)
            {
                __errors.Add("Length of property 'YearMonthPattern' cannot be greater than 100.");
            }
            if (ApplicationLanguage == null)
            {
                __errors.Add("Association with 'ApplicationLanguage' is required.");
            }
            if (throwException && __errors.Any())
            {
                throw new BusinessException("An instance of TypeClass 'DateTimeFormat' has validation errors:\r\n\r\n" + string.Join("\r\n", __errors));
            }
            return __errors;
        }

        public virtual int _GetUniqueIdentifier()
        {
            var hashCode = 399326290;
            hashCode = hashCode * -1521134295 + (Id?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (LongDatePattern?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (LongTimePattern?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (MonthDayPattern?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (RFC1123Pattern?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (ShortDatePattern?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (ShortTimePattern?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (YearMonthPattern?.GetHashCode() ?? 0);
            return hashCode;
        }






/// <summary>
/// Copies the current object to a new instance
/// </summary>
/// <param name="deep">Copy members that refer to objects external to this class (not dependent)</param>
/// <param name="copiedObjects">Objects that should be reused</param>
/// <param name="asNew">Copy the current object as a new one, ready to be persisted, along all its members.</param>
/// <param name="reuseNestedObjects">If asNew is true, this flag if set, forces the reuse of all external objects.</param>
/// <param name="copy">Optional - An existing [DateTimeFormat] instance to use as the destination.</param>
/// <returns>A copy of the object</returns>
        public virtual DateTimeFormat Copy(bool deep=false, Hashtable copiedObjects=null, bool asNew=false, bool reuseNestedObjects = false, DateTimeFormat copy = null)
        {
            if(copiedObjects == null)
            {
                copiedObjects = new Hashtable();
            }
            if (copy == null && copiedObjects.Contains(this))
                return (DateTimeFormat)copiedObjects[this];
            copy = copy ?? new DateTimeFormat();
            if (!asNew)
            {
                copy.TransientId = this.TransientId;
                copy.Id = this.Id;
            }
            copy.LongDatePattern = this.LongDatePattern;
            copy.LongTimePattern = this.LongTimePattern;
            copy.MonthDayPattern = this.MonthDayPattern;
            copy.RFC1123Pattern = this.RFC1123Pattern;
            copy.ShortDatePattern = this.ShortDatePattern;
            copy.ShortTimePattern = this.ShortTimePattern;
            copy.YearMonthPattern = this.YearMonthPattern;
            if (!copiedObjects.Contains(this))
            {
                copiedObjects.Add(this, copy);
            }
            if(deep && this.applicationLanguage != null)
            {
                if (!copiedObjects.Contains(this.applicationLanguage))
                {
                    if (asNew && reuseNestedObjects)
                        copy.ApplicationLanguage = this.ApplicationLanguage;
                    else if (asNew)
                        copy.ApplicationLanguage = this.ApplicationLanguage.Copy(deep, copiedObjects, true);
                    else
                        copy.applicationLanguage = this.applicationLanguage.Copy(deep, copiedObjects, false);
                }
                else
                {
                    if (asNew)
                        copy.ApplicationLanguage = (ApplicationLanguage)copiedObjects[this.ApplicationLanguage];
                    else
                        copy.applicationLanguage = (ApplicationLanguage)copiedObjects[this.ApplicationLanguage];
                }
            }
            return copy;
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as DateTimeFormat;
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
        public static bool operator ==(DateTimeFormat x, DateTimeFormat y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Equals(x, y);
        }

// Maintain inequality operator semantics for entities.
        public static bool operator !=(DateTimeFormat x, DateTimeFormat y)
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
        protected bool HasSameNonDefaultIdAs(DateTimeFormat compareTo)
        {
            return !this.IsTransient() && !compareTo.IsTransient() && this.Id.Equals(compareTo.Id);
        }

        #endregion


    }
}
