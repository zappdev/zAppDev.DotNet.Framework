using zAppDev.DotNet.Framework.Data.Encryption.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace zAppDev.DotNet.Framework.Data.Encryption.Types
{
    /// <summary>
    /// Hashes a Float when it's saved
    /// and salt hashes it when it gets it from the database.
    /// </summary>
    public class EncryptedFloat : EncryptedType
    {
        /// <summary>
        /// The returned type is a <see cref="float"/>
        /// </summary>
        public override Type ReturnedType
        {
            get
            {
                return (NotNull == true) ? typeof(float) : typeof(float?);
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
                return value == null ? 0.ToString("R", CultureInfo.InvariantCulture) : ((float)value).ToString("R", CultureInfo.InvariantCulture);
            }
            return value == null ? null : ((float)value).ToString("R", CultureInfo.InvariantCulture);
        }

        public override object GetValue(string value)
        {
            float result;
            if (float.TryParse(value, NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                return result;
            return null;
        }
    }
}
