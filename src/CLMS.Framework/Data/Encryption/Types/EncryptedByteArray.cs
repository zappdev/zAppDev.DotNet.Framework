using CLMS.Framework.Data.Encryption.Helpers;
using System;
using System.Collections.Generic;

namespace CLMS.Framework.Data.Encryption.Types
{
    /// <summary>
    /// Hashes an Integer when it's saved
    /// and salt hashes it when it gets it from the database.
    /// </summary>
    public class EncryptedByteArray : EncryptedType
    {
        /// <summary>
        /// The returned type is a <see cref="int"/>
        /// </summary>
        public override Type ReturnedType
        {
            get
            {
                return typeof(byte[]);
            }
        }

        public override string GetString(object value)
        {
            if (this.NotNull == true && value == null)
            {
                return Convert.ToBase64String(new byte[0]);
            }
            return value == null ? null : Convert.ToBase64String((byte[])value);
        }

        public override object GetValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return Convert.FromBase64String(value);
        }

        public override void SetParameterValues(IDictionary<string, string> parameters)
        {
            var parametersManager = new MappingParametersHelper(parameters);
            NotNull = parametersManager.GetBooleanParameterValue(MappingParametersHelper.NOT_NULL_PARAMETER_NAME);
            Length = parametersManager.GetIntegerParameterValue(MappingParametersHelper.LENGTH_PARAMETER_NAME);
        }
    }
}
