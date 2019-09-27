using System;

namespace zAppDev.DotNet.Framework.Logging
{
#if NETFRAMEWORK
#else
    public class Log4ApiNetLogger : IAPILogger
    {
        public void LogExposedAPIAccess(string controller, string action, Guid requestId, TimeSpan processingTime, bool cacheHit)
        {
            
        }
    }
#endif
}
