// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using zAppDev.DotNet.Framework.Data.Encryption.Manager;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace zAppDev.DotNet.Framework.Data.Encryption.Types
{
    /// <summary>
    /// Hashes a string when it's saved
    /// and salt hashes it when it gets it from the database.
    /// </summary>
    public abstract class EncryptedType : IUserType, IParameterizedType
    {
        public bool? NotNull
        {
            get;
            set;
        }

        public int? Length
        {
            get;
            set;
        }

        public abstract void SetParameterValues(IDictionary<string, string> parameters);

        public abstract string GetString(object value);
        public abstract object GetValue(string value);

        /// <summary>
        /// The returned type is a <see cref="string"/>
        /// </summary>
        public abstract Type ReturnedType
        {
            get;
        }

        /// <summary>
        /// Retrieve an instance of the mapped class from a Ado.Net resultset.
        /// Implementors should handle possibility of null values.
        /// </summary>
        /// <param name="rs"></param>
        /// <param name="names"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public virtual object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            var passwordString = NHibernateUtil.Binary.NullSafeGet(rs, names[0], session);
            var result = passwordString != null ? EncryptionManagerBase.Instance.DecryptObject((byte[])passwordString) : null;
            return GetValue(result);
        }

        /// <summary>
        /// Write an instance of the mapped class to a prepared statement.
        /// Handle possibility of null values.
        /// A multi-column type should be written to parameters starting from index.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="value"></param>
        /// <param name="index"></param>
        public virtual void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            if (value == null)
            {
                NHibernateUtil.String.NullSafeSet(cmd, null, index, session);
                return;
            }
            var hashedPassword = EncryptionManagerBase.Instance.EncryptObject(GetString(value));
            NHibernateUtil.Binary.NullSafeSet(cmd, hashedPassword, index, session);
        }

        /// <summary>
        /// Return a deep copy of the persistent state,
        /// stopping at entities and at collections.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual object DeepCopy(object value)
        {
            return value == null ? null : value;
        }




        /// <summary>
        /// During merge, replace the existing (target) value in the entity we are
        /// merging to with a new (original) value from the detached entity we are
        /// merging. For immutable objects, or null values, it is safe to simply
        /// return the first parameter. For mutable objects, it is safe to return a
        /// copy of the first parameter. For objects with component values, it might
        /// make sense to recursively replace component values.
        /// </summary>
        /// <param name="original">the value from the detached entity being merged</param>
        /// <param name="target">the value in the managed entity</param>
        /// <param name="owner">the managed entity</param>
        /// <returns>Returns the first parameter because it is inmutable</returns>
        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        /// <summary>
        /// Reconstruct an object from the cacheable representation.
        /// At the very least this method should perform a deep copy if the type is mutable.
        /// (optional operation)
        /// </summary>
        /// <param name="cached">the object to be cached</param>
        /// <param name="owner">the owner of the cached object</param>
        /// <returns>a reconstructed string from the cachable representation</returns>
        public object Assemble(object cached, object owner)
        {
            return DeepCopy(cached);
        }

        /// <summary>
        /// Transform the object into its cacheable representation.
        /// At the very least this method should perform a deep copy if the type is mutable.
        /// That may not be enough for some implementations, however;
        /// for example, associations must be cached as identifier values.
        /// (optional operation)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public object Disassemble(object value)
        {
            return DeepCopy(value);
        }

        /// <summary>
        /// The SQL types for the columns mapped by this type.
        /// In this case just a SQL Type will be returned:<seealso cref="DbType.String"/>
        /// </summary>
        public virtual SqlType[] SqlTypes
        {
            get
            {
                return new[] { new SqlType(System.Data.DbType.Binary, Length.Value) };
            }
        }

        /// <summary>
        /// The strings are not mutables.
        /// </summary>
        public bool IsMutable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Compare two <see cref="string"/>
        /// </summary>
        /// <param name="x">string to compare 1</param>
        /// <param name="y">string to compare 2</param>
        /// <returns>If are equals or not</returns>
        public new bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            return x.Equals(y);
        }

        public int GetHashCode(object x)
        {
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }
            return x.GetHashCode();
        }
    }
}
