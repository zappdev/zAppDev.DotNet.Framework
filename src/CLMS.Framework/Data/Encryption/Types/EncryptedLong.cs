using CLMS.Framework.Data.Encryption.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CLMS.Framework.Data.Encryption.Types
{
    /// <summary>
    /// Hashes a Long when it's saved
    /// and salt hashes it when it gets it from the database.
    /// </summary>
    public class EncryptedLong : EncryptedType
    {
        /// <summary>
        /// The returned type is a <see cref="long"/>
        /// </summary>
        public override Type ReturnedType
        {
            get
            {
                return (NotNull == true) ? typeof(long) : typeof(long?);
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
                return value == null ? "0" : ((long)value).ToString(CultureInfo.InvariantCulture);
            }
            return value == null ? null : ((long)value).ToString(CultureInfo.InvariantCulture);
        }

        public override object GetValue(string value)
        {
            long result;
            if (long.TryParse(value, NumberStyles.Number | NumberStyles.Integer | NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                return result;
            return null;
        }
    }
}
