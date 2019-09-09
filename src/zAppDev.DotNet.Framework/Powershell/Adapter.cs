// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Powershell
{
    public class Adapter<T>
    {
        public InvocationResults<T> GetPowershellResults(string scriptName, Dictionary<string, object> scriptArguments = null)
        {
            var result = new InvocationResults<T>();
            try
            {
                var psObjects = Commander.RunPowerShellScript(scriptName, scriptArguments);
                var results = psObjects.Convert<T>();
                result.Successful = results != null && results != null;
                result.Result = results;
            }
            catch (Exception e)
            {
                throw e;
            }

            return result;

        }//end GetPowershellResults()

        public InvocationResult<T> GetPowershellResult(string scriptName, Dictionary<string, object> scriptArguments = null)
        {
            var result = new InvocationResult<T>();
            try
            {
                var psObjects = Commander.RunPowerShellScript(scriptName, scriptArguments);
                if(psObjects.Count > 0)
                {
                    result.Result = psObjects[0].Convert<T>();
                    result.Successful = result.Result != null && result.Result.GetType() == typeof(T);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return result;
        }//end GetPowershellResults()
    }//end public class PowerShellExtensions
}
