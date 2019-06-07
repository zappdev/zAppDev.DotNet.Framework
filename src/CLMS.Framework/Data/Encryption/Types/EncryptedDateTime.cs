using CLMS.Framework.Data.Encryption.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CLMS.Framework.Data.Encryption.Types
{
    /// <summary>
    /// Hashes a DateTime when it's saved
    /// and salt hashes it when it gets it from the database.
    /// </summary>
    public class EncryptedDateTime : EncryptedType
    {
        /// <summary>
        /// The returned type is a <see cref="DateTime"/>
        /// </summary>
        public override Type ReturnedType
        {
            get
            {
                return (NotNull == true) ? typeof(DateTime) : typeof(DateTime?);
            }
        }

        public override string GetString(object value)
        {
            try
            {
                if(value != null)
                {
                    return new System.Data.SqlTypes.SqlDateTime((DateTime)value).Value.Ticks.ToString(CultureInfo.InvariantCulture);
                }
                if (this.NotNull == true)
                {
                    return System.Data.SqlTypes.SqlDateTime.MinValue.Value.Ticks.ToString(CultureInfo.InvariantCulture);
                }
                return null;
            }
            catch (Exception)
            {
                return System.Data.SqlTypes.SqlDateTime.MinValue.Value.Ticks.ToString(CultureInfo.InvariantCulture);
            }
        }

        public override object GetValue(string value)
        {
            long ticks;
            if (long.TryParse(value, NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture, out ticks))
            {
                return new DateTime(ticks);
            }
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
