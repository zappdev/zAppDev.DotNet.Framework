using System;
using System.Web.Http;
using CLMS.Framework.Data;

namespace CLMS.Framework.Auditing
{
    public class NHAuditTrailService
    {
        public static INHAuditTrailManager GetInstance()
        {
            return GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(INHAuditTrailManager)) as INHAuditTrailManager;
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
