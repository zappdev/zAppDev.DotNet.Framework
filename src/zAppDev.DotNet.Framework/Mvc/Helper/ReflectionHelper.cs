using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace zAppDev.DotNet.Framework.Mvc
{
    public static class ReflectionHelper
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jToken">The JToken containing the parameters of the reflected method to be called</param>
        /// <param name="methodParameters">The actual parameters of the reflected method to be called. Required when <paramref name="doConvert"/> is True</param>.
        /// <param name="doConvert">
        ///     If true, the parameters denoted in the <paramref name="jToken"/> will be converted to the respective types as seen in <paramref name="methodParameters"/>. 
        ///     Otherwise, they will be used as plain JSON strings
        /// </param>        
        /// <returns>
        ///     An array containing the required (and converted, if needed) parameters, as denoted in their expected <paramref name="methodParameters"/> representation.
        ///     Null, if <paramref name="methodParameters"/> is empty or an exception is suppressed.
        /// </returns>
        /// <exception cref="ArgumentNullException">When <paramref name="methodParameters"/> is null.</exception>
        private static object[] ToParametersArray(this JToken jToken, ParameterInfo[] methodParameters, bool doConvert)
        {
            if (methodParameters == null) throw new ArgumentNullException(nameof(methodParameters));
            if (methodParameters.Length == 0) return null;

            var parameters = new object[methodParameters.Length];
            int parameterIndex = 0;
            foreach (var token in jToken.Children())
            {
#if NETFRAMEWORK
                var parameterTypeFullName = System.Web.Compilation.BuildManager.GetType(methodParameters[parameterIndex].ParameterType.FullName, false, false);
#else
                var parameterTypeFullName = Assembly.GetEntryAssembly().DefinedTypes.Where(type => type.FullName.Equals(methodParameters[parameterIndex].ParameterType.FullName)).First();
#endif


                var stringToken = token.ToString();
                Utilities.DeSanitizeSerializedData(ref stringToken);
                if (doConvert)
                {
                    var slash = "";
                    if (!token.HasValues)
                    {
                        slash = "\"";
                        stringToken = stringToken.Replace(slash, "");
                    }

                    parameters[parameterIndex] = JsonConvert.DeserializeObject($"{slash}{stringToken}{slash}", parameterTypeFullName);

                }
                else
                {
                    parameters[parameterIndex] = stringToken;
                }
                parameterIndex++;
            }
            return parameters;
        }


        /// <summary>
        /// Invokes any static method that returns void
        /// </summary>
        /// <param name="assemblyName">The Namespace of the Method you wish to invoke (e.g. "MyApplication.Hubs")</param>
        /// <param name="methodName">The Name of the Method you wish to invoke (e.g. "RaiseDebugMessage")</param>
        /// <param name="jToken">The JToken fetched in the POST body of the called Controller containing the parameters of the <paramref name="methodName"/> method call (e.g. _LoadViewModel()["parameters"])</param>
        /// <param name="convertParameters">
        ///     If true, the parameters denoted in the <paramref name="jToken"/> will be converted to the respective types required by the method of <paramref name="methodName"/>. 
        ///     Otherwise, they will be used as plain JSON strings
        /// </param>
        /// <param name="throwException">If false, will suppress all exceptions.</param>
        public static void InvokeStaticVoidMethod(string assemblyName, string methodName, JToken jToken, bool convertParameters, bool throwException = false)
        {
            try
            {
                var referencedAssemblies = Assembly.GetCallingAssembly().GetReferencedAssemblies();

                Assembly requestedAssembly = null;

                foreach (var referencedAssembly in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
                {
                    if (referencedAssembly.Name == assemblyName)
                    {
                        requestedAssembly = Assembly.Load(referencedAssembly);
                        break;
                    }
                }

                if (requestedAssembly == null) throw new DllNotFoundException($"Requested Assembly [{assemblyName}] not found.");


                MethodInfo requestedMethod = null;
                foreach (var exportedType in requestedAssembly.GetExportedTypes())
                {
                    requestedMethod = exportedType.GetMethod(methodName);
                    if (requestedMethod != null) break;
                }

                if (requestedMethod == null) throw new KeyNotFoundException($"Requested Method [{methodName}] not found in Assembly [{assemblyName}]");

                requestedMethod.Invoke(null, jToken.ToParametersArray(requestedMethod.GetParameters(), convertParameters));
            }
            catch (Exception)
            {
                if (throwException) throw;
            }
        }//end InvokeStaticVoidMethod()
    }
}