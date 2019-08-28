#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.Filters;
using WebApi.OutputCache.Core;
using WebApi.OutputCache.Core.Time;

namespace zAppDev.DotNet.Framework.Mvc
{
    public class MambaDefinedShortTime : IModelQuery<DateTime, CacheTime>
    {
        private double serverTimeInSeconds;
        private double clientTimeInSeconds;
        private double? sharedTimeInSecounds;
        private MethodInfo _customMethod;
        private object[] _invocationParameters;

        public MambaDefinedShortTime(HttpActionExecutedContext actionExecutedContext, MethodInfo customMethod)
        {
            _customMethod = customMethod;

            var methodParameters = _customMethod.GetParameters();
            _invocationParameters = new object[methodParameters.Length];
            var actionArguments = actionExecutedContext.ActionContext.ActionArguments;
            var index = 0;

            foreach (var methodParameter in methodParameters)
            {
                if (methodParameter.Name == "response")
                {
                    _invocationParameters[index] = ((System.Net.Http.ObjectContent)actionExecutedContext.Response.Content).Value;
                }
                else
                {
                    var actionArgument = actionArguments.FirstOrDefault(x => x.Key == methodParameter.Name);
                    if (!actionArgument.Equals(default(KeyValuePair<string, object>))) _invocationParameters[index] = actionArgument.Value;
                }
                index++;
            }
        }

        private void EvaluateTimespan()
        {
            TimeSpan timeSpan = (TimeSpan) _customMethod.Invoke(null, _invocationParameters);

            serverTimeInSeconds = timeSpan.TotalSeconds;
            clientTimeInSeconds = serverTimeInSeconds;

            if (serverTimeInSeconds < 0)
                serverTimeInSeconds = 0;

            if (clientTimeInSeconds < 0)
                clientTimeInSeconds = 0;

            if (sharedTimeInSecounds.HasValue && sharedTimeInSecounds.Value < 0)
                sharedTimeInSecounds = 0;
        }

        public CacheTime Execute(DateTime model)
        {
            EvaluateTimespan();

            //this is my Execute!!!!
            var cacheTime = new CacheTime
            {
                AbsoluteExpiration = model.AddSeconds(serverTimeInSeconds),
                ClientTimeSpan = TimeSpan.FromSeconds(clientTimeInSeconds),
                SharedTimeSpan = sharedTimeInSecounds.HasValue ? (TimeSpan?)TimeSpan.FromSeconds(sharedTimeInSecounds.Value) : null
            };

            return cacheTime;
        }
    }
}
#endif