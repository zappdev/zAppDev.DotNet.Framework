using System.Threading;
using System.Threading.Tasks;
using NHibernate.Event;

namespace CLMS.Framework.Auditing
{
    public class NHAuditTrailListener : IPostUpdateEventListener, IPostInsertEventListener, IPostDeleteEventListener, IPreCollectionUpdateEventListener
    {
#if NETFRAMEWORK
        protected INHAuditTrailManager AuditTrailManager => System.Web.Http.GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(INHAuditTrailManager)) as INHAuditTrailManager;
#else
        protected INHAuditTrailManager AuditTrailManager => CLMS.Framework.Utilities.ServiceLocator.Current.GetInstance<INHAuditTrailManager>();
#endif

        public Task OnPostUpdateAsync(PostUpdateEvent @event, CancellationToken cancellationToken)
        {
            return Task.Run(() => OnPostUpdate(@event), cancellationToken);
        }

        public void OnPostUpdate(PostUpdateEvent @event)
        {
            AuditTrailManager?.OnPostUpdateLogEvent(@event);
        }

        public Task OnPostInsertAsync(PostInsertEvent @event, CancellationToken cancellationToken)
        {
            return Task.Run(() => OnPostInsert(@event), cancellationToken);
        }

        public void OnPostInsert(PostInsertEvent @event)
        {
            AuditTrailManager?.OnPostInsertLogEvent(@event);
        }

        public Task OnPostDeleteAsync(PostDeleteEvent @event, CancellationToken cancellationToken)
        {
            return Task.Run(() => OnPostDelete(@event), cancellationToken);
        }

        public void OnPostDelete(PostDeleteEvent @event)
        {
            AuditTrailManager?.OnPostDeleteLogEvent(@event);
        }

        public Task OnPreUpdateCollectionAsync(PreCollectionUpdateEvent @event, CancellationToken cancellationToken)
        {
            return Task.Run(() => OnPreUpdateCollection(@event), cancellationToken);
        }

        public void OnPreUpdateCollection(PreCollectionUpdateEvent @event)
        {
            AuditTrailManager?.OnPreCollectionUpdateLogEvent(@event);
        }
    }
}