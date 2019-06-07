using System;
using System.Collections.Generic;
using CLMS.Framework.Data.Encryption.Helpers;

namespace CLMS.Framework.Data.Encryption.Types
{
    /// <summary>
    /// Hashes a Byte when it's saved
    /// and salt hashes it when it gets it from the database.
    /// </summary>
    public class EncryptedByte : EncryptedType
    {
        /// <summary>
        /// The returned type is a <see cref="Byte"/>
        /// </summary>
        public override Type ReturnedType
        {
            get
            {
                return (NotNull == true) ? typeof(Byte) : typeof(Byte?);
            }
        }

        public override void SetParameterValues(IDictionary<string, string> parameters)
        {
            var mappingParametersHelper = new MappingParametersHelper(parameters);
            NotNull = mappingParametersHelper.GetBooleanParameterValue(MappingParametersHelper.NOT_NULL_PARAMETER_NAME);
            Length = mappingParametersHelper.GetIntegerParameterValue(MappingParametersHelper.LENGTH_PARAMETER_NAME);
        }

        public override string GetString(object value)
        {
            if (this.NotNull == true)
            {
                return value == null ? "" : ((Byte)value).ToString();
            }
            return value == null ? null : ((Byte)value).ToString();
        }

        public override object GetValue(string value)
        {
            Byte result;
            if (Byte.TryParse(value, out result))
                return result;
            return null;
        }
    }
}
