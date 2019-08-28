#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Event;
using NHibernate.Persister.Collection;
using System.Threading;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Identity.Model;

namespace zAppDev.DotNet.Framework.Identity
{
    public class NHAuthorizationListener : IPostUpdateEventListener, IPostInsertEventListener, IPostDeleteEventListener, IPreCollectionUpdateEventListener
    {
        private string[] _authorizationObjects =
        {
            nameof(ApplicationOperation),
            nameof(ApplicationPermission),
            nameof(ApplicationRole)
        };

        public Task OnPreUpdateCollectionAsync(PreCollectionUpdateEvent @event, CancellationToken cancellationToken)
        {
            return Task.Run(() => OnPreUpdateCollection(@event), cancellationToken);
        }

        public void OnPreUpdateCollection(PreCollectionUpdateEvent @event)
        {
            var affectedOwnerEntityName = @event.GetAffectedOwnerEntityName();
            //var role = @event.Collection.Role;
            if (_authorizationObjects.Any(a => a == affectedOwnerEntityName)
                    && @event.Collection.IsDirty)
            {
                AuthorizationManager.InvalidateCache();
            }
        }

        public Task OnPostUpdateAsync(PostUpdateEvent @event, CancellationToken cancellationToken)
        {
            return Task.Run(() => OnPostUpdate(@event), cancellationToken);
        }

        public void OnPostUpdate(PostUpdateEvent @event)
        {
            var name = @event.Entity.GetType().Name;
            if (_authorizationObjects.Any(a => a == name))
            {
                AuthorizationManager.InvalidateCache();
            }
        }

        public Task OnPostInsertAsync(PostInsertEvent @event, CancellationToken cancellationToken)
        {
            return Task.Run(() => OnPostInsert(@event), cancellationToken);
        }

        public void OnPostInsert(PostInsertEvent @event)
        {
            var name = @event.Entity.GetType().Name;
            if (_authorizationObjects.Any(a => a == name))
            {
                AuthorizationManager.InvalidateCache();
            }
        }

        public Task OnPostDeleteAsync(PostDeleteEvent @event, CancellationToken cancellationToken)
        {
            return Task.Run(() => OnPostDelete(@event), cancellationToken);
        }

        public void OnPostDelete(PostDeleteEvent @event)
        {
            var name = @event.Entity.GetType().Name;
            if (_authorizationObjects.Any(a => a == name))
            {
                AuthorizationManager.InvalidateCache();
            }
        }
    }
}
#endif