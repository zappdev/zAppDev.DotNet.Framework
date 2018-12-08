using System;
using CLMS.Framework.Data;

#if NETFRAMEWORK            
using System.Web.Http;
#else
using CLMS.Framework.Utilities;
#endif

namespace CLMS.Framework.Auditing
{
    public class NHAuditTrailService
    {
        public static INHAuditTrailManager GetInstance()
        {
            #if NETFRAMEWORK            
            return GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(INHAuditTrailManager)) as INHAuditTrailManager;
            #else
            return ServiceLocator.Current.GetInstance<INHAuditTrailManager>();
            #endif
        }

        public static void ExecuteWithoutAuditTrail(Func<object, object> action)
        {
            var audit = GetInstance();

            try {
                audit.IsTemporarilyDisabled = true;

                MiniSessionManager.ExecuteInUoW(manager =>
                {
                    var result = action?.Invoke(audit);
                });

                audit.IsTemporarilyDisabled = false;
            } catch (Exception) {
                audit.IsTemporarilyDisabled = false;
                throw;
            }
        }
    }
}
