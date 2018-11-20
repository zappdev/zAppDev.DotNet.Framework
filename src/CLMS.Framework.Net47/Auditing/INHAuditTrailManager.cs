using NHibernate.Event;

namespace CLMS.Framework.Auditing
{
    public interface INHAuditTrailManager
    {
        bool IsTemporarilyDisabled { get; set; }

    	void InitializeConfiguration();
        void OnPostUpdateLogEvent(PostUpdateEvent postUpdateEvent);
        void OnPostInsertLogEvent(PostInsertEvent postInsertEvent);
        void OnPostDeleteLogEvent(PostDeleteEvent postDeleteEvent);
        void OnPreCollectionUpdateLogEvent(PreCollectionUpdateEvent preCollectionUpdateEvent);
    }
}
