// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using zAppDev.DotNet.Framework.Utilities;
using log4net;
using NHibernate;
using NHibernate.Cfg;
using zAppDev.DotNet.Framework.Data.DAL;
using NHibernate.Cfg.MappingSchema;
using zAppDev.DotNet.Framework.Data.DatabaseManagers;

#if NETFRAMEWORK
using zAppDev.DotNet.Framework.Owin;
using Microsoft.AspNet.Identity.Owin;
#endif

namespace zAppDev.DotNet.Framework.Data
{
    public class MiniSessionManager:IDisposable
    {
        public const string SessionKeyName = "NHibernateSession";

        private readonly ILog _log = LogManager.GetLogger(typeof(MiniSessionManager));
        [ThreadStatic]
        private static MiniSessionManager _manager;
        private static ISessionFactory _sessionFactory;
        private static NHibernate.Cfg.Configuration _config;

        public static string UpdateDbScript
        {
            get;
            private set;
        }
        public static string UpdateDbErrors
        {
            get;
            private set;
        }
        public static string OwinKey
        {
            get
            {
                var type = typeof (MiniSessionManager);
                return "AspNet.Identity.Owin:" + type.AssemblyQualifiedName;
            }
        }

        public MiniSessionManager()
        {
            InstanceId = Guid.NewGuid();
        }

        public Guid InstanceId
        {
            get;
        }

        private static ISessionFactory BuildSessionFactory()
        {
            _config = new NHibernate.Cfg.Configuration();

            var hibernateConfig = "hibernate.cfg.xml";
            //if not rooted, assume path from base directory
            if (System.IO.Path.IsPathRooted(hibernateConfig) == false)
                hibernateConfig = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, hibernateConfig);
            if (System.IO.File.Exists(hibernateConfig))
			{
				using (var xmlTextReader = new System.Xml.XmlTextReader(hibernateConfig))
                {
                    _config.Configure(xmlTextReader);
                }
			}
            else
            {
                // Look for config section in web.config
                _config.Configure();
            }

            ServiceLocator.Current.GetInstance<IDatabaseManager>().RemoveSchemas(_config);

            var factory = _config.BuildSessionFactory();

            //UpdateDatabaseSchema(_config);

            ObjectGraphWalker.Initialize(_config);

            return factory;
        }
 
        private static void UpdateDatabaseSchema(NHibernate.Cfg.Configuration cfg)
        {
            ServiceLocator.Current.GetInstance<IDatabaseManager>().CreateSchemas();

            var schemaUpdate = new NHibernate.Tool.hbm2ddl.SchemaUpdate(cfg);
            var updateCode = new System.Text.StringBuilder();

            schemaUpdate.Execute(row =>
            {
                updateCode.AppendLine(row);
                updateCode.AppendLine();
            }, true);


            UpdateDbScript = updateCode.ToString();
            UpdateDbErrors = string.Join("\n", schemaUpdate.Exceptions.Select(e => e.Message));
            DatabaseUpdateExecutedInRequest = true;
        }

        public static void ExecuteScript(string stmt)
        {
            ExecuteScript(System.Configuration.ConfigurationManager.ConnectionStrings["Database"].ConnectionString, stmt);
        }

        public static void ExecuteScript(string connectionString, string stmt)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                if (string.IsNullOrEmpty(stmt)) return;
                using (var cm = connection.CreateCommand())
                {
                    cm.CommandType = CommandType.Text;
                    cm.CommandText = stmt;
                    cm.ExecuteNonQuery();
                }
            }
        }

        #if NETFRAMEWORK
        public static MiniSessionManager TryGetInstance()
        {
            return InstanceSafe;
        }

        public static MiniSessionManager InstanceSafe => 
            HttpContext.Current?.Items["owin.Environment"] == null
            ? _manager
            : OwinHelper.GetOwinContext(HttpContext.Current)?.Get<MiniSessionManager>();

        #endif

        public static MiniSessionManager Instance
        {
            get
            {
                if (_manager != null)
                {
                    return _manager;
                }

                #if NETFRAMEWORK
                if (HttpContext.Current?.Items["owin.Environment"] != null)
                {
                    var manager = OwinHelper.GetOwinContext(HttpContext.Current)?.Get<MiniSessionManager>();
                    if (manager == null)
                    {
                        throw new ApplicationException("Could not find MiniSessionManager in OWIN context!");
                    }
                    return manager;
                }
                #endif
                throw new ApplicationException("There is no Instance of MiniSessionManager!!!");
            }
            private set
            {
                // Only executed when in Single UoW
                // overrides existing value
                _manager = value;
            }
        }

        public ISession Session
        {
            get;
            set;
        }

        public bool? SingleUseSession
        {
            get;
            set;
        }

        public static ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    _sessionFactory = BuildSessionFactory();
                }
                if (DatabaseUpdateFailed && !DatabaseUpdateExecutedInRequest)
                {
                    UpdateDatabaseSchema(_config);
                }
                return _sessionFactory;
            }
        }

        /// <summary>
        /// Opens a new NH Session or reuses an already open one.
        /// </summary>
        /// <returns></returns>
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

        private void CloseSession()
        {
            Session?.Dispose();
            Session = null;
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
                _log.Error("Error in closing session.", x);
                throw;
            }
            finally
            {
                postAction?.Invoke();
                CloseSession();
            }
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
                _log.Warn("No session to rollback!", new ApplicationException("No Session to Rollback!!"));
                return;
            }
            if (Session.Transaction.IsActive)
            {
                Session.Transaction.Rollback();
            }
            CloseSession();
        }

        public static MiniSessionManager Create()
        {
            return new MiniSessionManager();
        }

        public bool WillFlush
        {
            get;
            set;
        }

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

        public static bool DatabaseUpdateFailed => !string.IsNullOrWhiteSpace(UpdateDbErrors);
        private static bool DatabaseUpdateExecutedInRequest
        {
            get
            {
                return Local.Data["DatabaseUpdateExecutedInRequest"] != null && (bool)Local.Data["DatabaseUpdateExecutedInRequest"];
            }
            set
            {
                Local.Data["DatabaseUpdateExecutedInRequest"] = value;
            }
        }

        ~MiniSessionManager()
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

#if NETFRAMEWORK
        public static T ExecuteInUoW<T>(Func<MiniSessionManager,T> func, MiniSessionManager mgr = null)
        {
            var prevManager = _manager;
            var manager = mgr ?? new MiniSessionManager();
            Instance = manager;
            try
            {
                manager.OpenSessionWithTransaction();
                T result = func(manager);
                manager.Commit();
                return result;
            }
            catch (Exception)
            {
                manager.Rollback();
                // This causes unhandled if called in a new thread!
                throw;
            }
            finally
            {
                if (mgr == null)
                    manager.Dispose();
                Instance = prevManager;
            }
        }

        public static void ExecuteInUoW(Action<MiniSessionManager> action, MiniSessionManager manager = null)
        {
            ExecuteInUoW<object>((mgr =>
            {
                action(mgr);
                return null;
            }), manager);
        }
#else
        public static T ExecuteInUoW<T>(Func<MiniSessionService, T> action)
        {
            var factory = ServiceLocator.Current.GetInstance<ISessionFactory>();
            using (var manager = new MiniSessionService(factory))
            {
                manager.OpenSessionWithTransaction();
                var result = action(manager);
                manager.CommitChanges();
                return result;
            }
        }

        public static void ExecuteInUoW(Action<MiniSessionService> action)
        {
            var factory = ServiceLocator.Current.GetInstance<ISessionFactory>();
            using (var manager = new MiniSessionService(factory))
            {
                manager.OpenSessionWithTransaction();
                action(manager);
                manager.CommitChanges();
            }
        }

#endif

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
