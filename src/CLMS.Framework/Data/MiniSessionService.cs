using Microsoft.Extensions.Logging;
using NHibernate;
using System;
using System.Data;

namespace CLMS.Framework.Data
{
    public class MiniSessionService : IDisposable, IMiniSessionService
    {
        private RepositoryAction _lastAction = RepositoryAction.NONE;

        public RepositoryAction LastAction
        {
            get
            {
                return _lastAction;
            }
            set
            {
                if (value == RepositoryAction.INSERT
                        || value == RepositoryAction.DELETE
                        || value == RepositoryAction.SAVE
                        || value == RepositoryAction.UPDATE)
                {
                    WillFlush = true;
                }
                _lastAction = value;
            }
        }

        public bool WillFlush { get; set; }

        public ISession Session { get; }

        public readonly ILogger<MiniSessionService> Logger;

        public MiniSessionService(
            ISession session,
            ILogger<MiniSessionService> logger)
        {
            Session = session;
            Logger = logger;
        }

        public ISession OpenTransaction()
        {
            Session.BeginTransaction();
            return Session;
        }

        public ITransaction BeginTransaction()
        {
            return Session.BeginTransaction(IsolationLevel.ReadCommitted);
        }

        private void Commit()
        {
            if (Session == null)
            {
                throw new ApplicationException("No Session to Commit!!");
            }
            if (WillFlush)
            {
                Session.Flush();
            }
            if (Session.Transaction.IsActive)
            {
                Session.Transaction.Commit();
            }
        }

        public void CommitChanges(Exception exception = null, Action postAction = null)
        {
            try
            {
                if (exception != null)
                    Rollback();
                else
                    Commit();
            }
            catch (Exception x)
            {
                Logger.LogError("Error in closing session.", x);
                throw;
            }
            finally
            {
                postAction?.Invoke();
                CloseSession();
            }
        }

        private void Rollback()
        {
            if (Session == null)
            {
                Logger.LogWarning("No session to rollback!", new ApplicationException("No Session to Rollback!!"));
                return;
            }
            if (Session.Transaction.IsActive)
            {
                Session.Transaction.Rollback();
            }
        }

        public void CloseSession()
        {
            Session?.Dispose();
        }

        ~MiniSessionService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                CloseSession();
            }
        }

    }
}
