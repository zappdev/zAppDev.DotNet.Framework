using CLMS.Framework.Data.Encryption.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CLMS.Framework.Data.Encryption.Types
{
    /// <summary>
    /// Hashes a Char when it's saved
    /// and salt hashes it when it gets it from the database.
    /// </summary>
    public class EncryptedChar : EncryptedType
    {
        /// <summary>
        /// The returned type is a <see cref="char"/>
        /// </summary>
        public override Type ReturnedType
        {
            get
            {
                return (NotNull == true) ? typeof(char) : typeof(char?);
            }
        }

        public override string GetString(object value)
        {
            if (this.NotNull == true)
            {
                return value == null ? "" : ((char)value).ToString(CultureInfo.InvariantCulture);
            }
            return value == null ? null : ((char)value).ToString(CultureInfo.InvariantCulture);
        }

        public override object GetValue(string value)
        {
            char result;
            if (char.TryParse(value, out result))
                return result;
            return null;
        }

        public override void SetParameterValues(IDictionary<string, string> parameters)
        {
            var mappingParametersHelper = new MappingParametersHelper(parameters);
            NotNull = mappingParametersHelper.GetBooleanParameterValue(MappingParametersHelper.NOT_NULL_PARAMETER_NAME);
            Length = mappingParametersHelper.GetIntegerParameterValue(MappingParametersHelper.LENGTH_PARAMETER_NAME);
        }
    }
}
