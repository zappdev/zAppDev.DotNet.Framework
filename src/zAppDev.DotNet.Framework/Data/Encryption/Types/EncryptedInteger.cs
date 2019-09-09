// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Data.Encryption.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace zAppDev.DotNet.Framework.Data.Encryption.Types
{
    /// <summary>
    /// Hashes an Integer when it's saved
    /// and salt hashes it when it gets it from the database.
    /// </summary>
    public class EncryptedInteger : EncryptedType
    {
        /// <summary>
        /// The returned type is a <see cref="int"/>
        /// </summary>
        public override Type ReturnedType
        {
            get
            {
                return (NotNull == true) ? typeof(int) : typeof(int?);
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
                return value == null ? 0.ToString(CultureInfo.InvariantCulture) : ((int)value).ToString(CultureInfo.InvariantCulture);
            }
            return value == null ? null : ((int)value).ToString(CultureInfo.InvariantCulture);
        }

        public override object GetValue(string value)
        {
            int result;
            if (int.TryParse(value, NumberStyles.Number | NumberStyles.Integer | NumberStyles.Float, CultureInfo.InvariantCulture, out result))
                return result;
            return null;
        }
    }
}
