// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Data.Encryption.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace zAppDev.DotNet.Framework.Data.Encryption.Types
{
    /// <summary>
    /// Hashes a Decimal when it's saved
    /// and salt hashes it when it gets it from the database.
    /// </summary>
    public class EncryptedDecimal : EncryptedType
    {
        public int? Precision
        {
            get;
            set;
        }
        public int? Scale
        {
            get;
            set;
        }
        /// <summary>
        /// The returned type is a <see cref="decimal"/>
        /// </summary>
        public override Type ReturnedType
        {
            get
            {
                return (NotNull == true) ? typeof(decimal) : typeof(decimal?);
            }
        }

        public override void SetParameterValues(IDictionary<string, string> parameters)
        {
            var parametersManager = new MappingParametersHelper(parameters);
            NotNull = parametersManager.GetBooleanParameterValue(MappingParametersHelper.NOT_NULL_PARAMETER_NAME);
            Precision = parametersManager.GetIntegerParameterValue(MappingParametersHelper.PRECISION_PARAMETER_NAME);
            Scale = parametersManager.GetIntegerParameterValue(MappingParametersHelper.SCALE_PARAMETER_NAME);
            Length = parametersManager.GetIntegerParameterValue(MappingParametersHelper.LENGTH_PARAMETER_NAME);
        }


        private string GetFormat()
        {
            string format = null;
            //precision = number of digits. scale = number of decimals
            if(Scale.HasValue)
            {
                format = "." + new string('0', Scale.Value);
            }
            if (Precision.HasValue)
            {
                int scale = Scale.HasValue ? Scale.Value : 0;
                format = new string('0', Precision.Value - scale) + format;
            }
            return format;
        }

        public override string GetString(object value)
        {
            if (this.NotNull == true)
            {
                return value == null ? 0.ToString(GetFormat(), CultureInfo.InvariantCulture) : ((decimal)value).ToString(GetFormat(), CultureInfo.InvariantCulture);
            }
            return value == null ? null : ((decimal)value).ToString(GetFormat(), CultureInfo.InvariantCulture);
        }

        public override object GetValue(string value)
        {
            decimal result;
            if (decimal.TryParse(value, NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                return result;
            return null;
        }
    }
}
