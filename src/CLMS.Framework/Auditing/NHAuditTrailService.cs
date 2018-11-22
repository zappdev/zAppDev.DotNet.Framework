using System;
using CLMS.Framework.Data;
using CLMS.Framework.Utilities;

namespace CLMS.Framework.Auditing
{
    public class NHAuditTrailService
    {
        public static INHAuditTrailManager GetInstance()
        {
            return ServiceLocator.Current.GetInstance<INHAuditTrailManager>();
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
