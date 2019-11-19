// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using Microsoft.Extensions.Logging;
using NHibernate;
using System;
using System.Data;
using zAppDev.DotNet.Framework.Utilities;
using zAppDev.DotNet.Framework.Data.DAL;

namespace zAppDev.DotNet.Framework.Data
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

        public ISession Session
        {
            get;
            set;
        }

        public ISessionFactory SessionFactory { get; }
        
        public readonly ILogger<MiniSessionService> Logger;

        public MiniSessionService(
            ISessionFactory factory,
            ILogger<MiniSessionService> logger = null)
        {
            SessionFactory = factory;
            Logger = logger;
        }

        public ISession OpenSession()
        {
            // Reuse session if exists
            if (Session != null && Session.IsOpen)
            {
                return Session;
            }
            var session = SessionFactory.OpenSession();
            session.FlushMode = FlushMode.Manual;
            Session = session;
            return session;
        }

        public ISession OpenSessionWithTransaction()
        {
            var session = OpenSession();
            session.BeginTransaction();
            return session;
        }

        public ISession OpenTransaction()
        {
            return OpenSession();
        }

        public ITransaction BeginTransaction()
        {
            return Session.BeginTransaction(IsolationLevel.ReadCommitted);
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
                Logger?.LogError("Error in closing session.", x);
                throw;
            }
            finally
            {
                postAction?.Invoke();
                CloseSession();
            }
        }
        
        public void CloseSession()
        {
            Session?.Dispose();
            Session = null;
        }

        ~MiniSessionService()
        {
            Dispose(false);
        }

        public static T ExecuteInUoW<T>(Func<MiniSessionService, T> action)
        {
            var factory = ServiceLocator.Current.GetInstance<ISessionFactory>();
            using (var manager = new MiniSessionService(factory))
            {
                return action(manager);
            }
        }

        public static void ExecuteInUoW(Action<MiniSessionService> action)
        {
            var factory = ServiceLocator.Current.GetInstance<ISessionFactory>();
            using (var manager = new MiniSessionService(factory))
            {
                action(manager);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
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

        private void Rollback()
        {
            if (Session == null)
            {
                Logger?.LogWarning("No session to rollback!", new ApplicationException("No Session to Rollback!!"));
                return;
            }
            if (Session.Transaction.IsActive)
            {
                Session.Transaction.Rollback();
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                CloseSession();
            }
        }
        public T ExecuteInTransaction<T>(Func<T> func)
        {
            try
            {
                this.OpenSessionWithTransaction();
                T result = func();
                this.Commit();
                return result;
            }
            catch (Exception)
            {
                this.Rollback();
                // This causes unhandled if called in a new thread!
                throw;
            }
        }

        public void ExecuteInTransaction(Action action)
        {
            ExecuteInTransaction<object>(() =>
            {
                action();
                return null;
            });
        }
    }
}

#endif