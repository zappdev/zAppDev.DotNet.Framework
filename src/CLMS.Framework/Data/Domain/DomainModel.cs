using System;
using System.Collections.Generic;

namespace CLMS.Framework.Data.Domain
{
    public class DomainModel: IDomainModelClass 
    {
        public virtual Guid TransientId { get; set; } = Guid.NewGuid();

        protected const int HashMultiplier = 31;
        protected int? cachedHashcode;


        public List<string> _Validate(bool throwException = true)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     When NHibernate proxies objects, it masks the type of the actual entity object.
        ///     This wrapper burrows into the proxied object to get its actual type.
        ///
        ///     Although this assumes NHibernate is being used, it doesn't require any NHibernate
        ///     related dependencies and has no bad side effects if NHibernate isn't being used.
        ///
        ///     Related discussion is at http://groups.google.com/group/sharp-architecture/browse_thread/thread/ddd05f9baede023a ...thanks Jay Oliver!
        /// </summary>
        protected virtual Type GetTypeUnproxied()
        {
            return GetType();
        }
    }
}
