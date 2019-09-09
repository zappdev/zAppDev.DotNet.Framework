// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Data.Encryption.Helpers
{
    public class MappingParametersHelper
    {
        private readonly IDictionary<string, string> _parameters;

        private bool _noParametersAvailable => _parameters == null || _parameters.Count == 0;

        public static string NOT_NULL_PARAMETER_NAME = "not-null";
        public static string LENGTH_PARAMETER_NAME = "length";
        public static string PRECISION_PARAMETER_NAME = "precision";
        public static string SCALE_PARAMETER_NAME = "scale";

        public MappingParametersHelper(IDictionary<string, string> parameters)
        {
            _parameters = parameters;
        }

        public bool? GetBooleanParameterValue(string parameterName)
        {
            if (_noParametersAvailable) return null;
            bool? result = null;
            bool value;
            if (_parameters.ContainsKey(parameterName) && bool.TryParse(_parameters[parameterName], out value))
            {
                result = value;
            }
            return result;
        }

        public int? GetIntegerParameterValue(string parameterName)
        {
            if (_noParametersAvailable) return null;
            int? result = null;
            int value;
            if (_parameters.ContainsKey(parameterName) && int.TryParse(_parameters[parameterName], out value))
            {
                result = value;
            }
            return result;
        }
    }
}
