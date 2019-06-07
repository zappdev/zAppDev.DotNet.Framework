using System;
using System.Collections.Generic;
using CLMS.Framework.Data.Encryption.Helpers;

namespace CLMS.Framework.Data.Encryption.Types
{
    /// <summary>
    /// Hashes a Boolean when it's saved
    /// and salt hashes it when it gets it from the database.
    /// </summary>
    public class EncryptedBoolean : EncryptedType
    {
        /// <summary>
        /// The returned type is a <see cref="bool"/>
        /// </summary>
        public override Type ReturnedType
        {
            get
            {
                return (NotNull == true) ? typeof(bool) : typeof(bool?);
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
            if(this.NotNull == true)
            {
                return value == null ? false.ToString() : ((bool)value).ToString();
            }
            return value == null ? null : ((bool)value).ToString();
        }

        public override object GetValue(string value)
        {
            bool result;
            if (bool.TryParse(value, out result))
                return result;
            return null;
        }
    }
}
