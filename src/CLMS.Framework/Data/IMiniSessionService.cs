using NHibernate;
using System;

namespace CLMS.Framework.Data
{
    public interface IMiniSessionService
    {
        RepositoryAction LastAction { get; set; }
        bool WillFlush { get; set; }
        ISession Session { get; }

        ITransaction BeginTransaction();
        void CloseSession();
        void Dispose();
        ISession OpenTransaction();

        void CommitChanges(Exception exception = null, Action postAction = null);
    }
}