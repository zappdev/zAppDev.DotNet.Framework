using zAppDev.DotNet.Framework.Data.Encryption.Helpers;
using System;
using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Data.Encryption.Types
{
    /// <summary>
    /// Hashes a Guid when it's saved
    /// and salt hashes it when it gets it from the database.
    /// </summary>
    public class EncryptedGuid : EncryptedType
    {
        /// <summary>
        /// The returned type is a <see cref="Guid"/>
        /// </summary>
        public override Type ReturnedType
        {
            get
            {
                return (NotNull == true) ? typeof(Guid) : typeof(Guid?);
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
                return value == null ? "" : ((Guid)value).ToString();
            }
            return value == null ? null : ((Guid)value).ToString();
        }

        public override object GetValue(string value)
        {
            Guid result;
            if (string.IsNullOrWhiteSpace(value)) return value;
            if (Guid.TryParse(value, out result))
                return result;
            return null;
        }
    }
}
