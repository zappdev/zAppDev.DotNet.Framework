// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using zAppDev.DotNet.Framework.Data.Encryption.Helpers;
using System.Globalization;

namespace zAppDev.DotNet.Framework.Data.Encryption.Types
{
    /// <summary>
    /// Hashes a Double when it's saved
    /// and salt hashes it when it gets it from the database.
    /// </summary>
    public class EncryptedDouble : EncryptedType
    {
        /// <summary>
        /// The returned type is a <see cref="double"/>
        /// </summary>
        public override Type ReturnedType
        {
            get
            {
                return (NotNull == true) ? typeof(double) : typeof(double?);
            }
        }

        public override string GetString(object value)
        {
            if (this.NotNull == true)
            {
                return value == null ? 0.ToString("R", CultureInfo.InvariantCulture) : ((double)value).ToString("R", CultureInfo.InvariantCulture);
            }
            return value == null ? null : ((double)value).ToString("R", CultureInfo.InvariantCulture);
        }

        public override object GetValue(string value)
        {
            double result;
            if (double.TryParse(value, NumberStyles.Number | NumberStyles.Float, CultureInfo.InvariantCulture, out result))
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
