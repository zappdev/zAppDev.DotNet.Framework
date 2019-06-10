using CLMS.Framework.Data.Encryption.Helpers;
using CLMS.Framework.Data.Encryption.Manager;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace CLMS.Framework.Data.Encryption.Types
{
    /// <summary>
    /// Hashes a String when it's saved
    /// and salt hashes it when it gets it from the database.
    /// </summary>
    public class EncryptedString : EncryptedType
    {

        /// <summary>
        /// Return a deep copy of the persistent state,
        /// stopping at entities and at collections.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override object DeepCopy(object value)
        {
            return value == null ? null : string.Copy((string)value);
        }

        /// <summary>
        /// The returned type is a <see cref="string"/>
        /// </summary>
        public override Type ReturnedType
        {
            get
            {
                return typeof(string);
            }
        }

        public override void SetParameterValues(IDictionary<string, string> parameters)
        {
            var parametersManager = new MappingParametersHelper(parameters);
            NotNull = parametersManager.GetBooleanParameterValue(MappingParametersHelper.NOT_NULL_PARAMETER_NAME);
            Length = parametersManager.GetIntegerParameterValue(MappingParametersHelper.LENGTH_PARAMETER_NAME);
        }

        public override string GetString(object value)
        {
            string result = (string)value;
            if(this.NotNull == true && result == null)
            {
                result = "";
            }
            if (string.IsNullOrEmpty(result)) return result;
            return result;
        }

        public override object GetValue(string value)
        {
            return value;
        }

        public override SqlType[] SqlTypes
        {
            get
            {
                return Length == null
                ? (new[] { new SqlType(System.Data.DbType.String) })
                : (new[] { new SqlType(System.Data.DbType.String, Length.Value) });
            }
        }

        public override object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            var passwordString = NHibernateUtil.String.NullSafeGet(rs, names[0], session);
            var result = passwordString != null ? EncryptionManagerBase.Instance.DecryptString((string)passwordString) : null;
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
        public override void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            if (value == null)
            {
                NHibernateUtil.String.NullSafeSet(cmd, null, index, session);
                return;
            }
            var hashedPassword = EncryptionManagerBase.Instance.EncryptString(GetString(value));
            NHibernateUtil.String.NullSafeSet(cmd, hashedPassword, index, session);
        }
    }
}
