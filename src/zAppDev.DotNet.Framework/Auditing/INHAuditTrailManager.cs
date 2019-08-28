using NHibernate.Event;
using System;
using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Auditing
{
    public interface INHAuditTrailManager
    {
        bool IsTemporarilyDisabled { get; set; }

        void ClearAuditTrailCache();
        void ExecuteWithoutAuditTrail(Func<object, object> action);
        void OnPostUpdateLogEvent(PostUpdateEvent postUpdateEvent);
        void OnPostInsertLogEvent(PostInsertEvent postInsertEvent);
        void OnPostDeleteLogEvent(PostDeleteEvent postDeleteEvent);
        void OnPreCollectionUpdateLogEvent(PreCollectionUpdateEvent preCollectionUpdateEvent);
        void Enable(List<Type> auditableTypes, Func<AuditContext> getAuditContext);
    }
}
