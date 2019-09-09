// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Data;
using zAppDev.DotNet.Framework.Data.DAL;
using log4net;
using NHibernate;
using System;

namespace zAppDev.DotNet.Framework.Workflow
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly ISession _currentSession;

        private readonly MiniSessionManager _sessionManager;

        private RepositoryAction? _prevAction;

        public void DeleteWorkflowContextBase(WorkflowContextBase workflowcontextbase, bool doNotCallDeleteForThis = false, bool isCascaded = false, object calledBy = null)
        {
            if (workflowcontextbase == null || workflowcontextbase.IsTransient()) return;
            if (!doNotCallDeleteForThis) Delete(workflowcontextbase, isCascaded);
        }
        public void DeleteWorkflowSchedule(WorkflowSchedule workflowschedule, bool doNotCallDeleteForThis = false, bool isCascaded = false, object calledBy = null)
        {
            if (workflowschedule == null || workflowschedule.IsTransient()) return;
            if (!doNotCallDeleteForThis) Delete(workflowschedule, isCascaded);
        }

        public void Delete<T>(T entity, bool isCascaded = false) where T : class
        {
            SetCurrentActionTo(RepositoryAction.DELETE);

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "No " + typeof(T).Name + " was specified.");
            }

            try
            {
                _currentSession.Delete(entity);
            }
            catch (Exception e)
            {
                var log = LogManager.GetLogger(typeof(T));
                log.Error("Error deleting", e);
                throw;
            }

            RestoreLastAction();
        }

        private void SetCurrentActionTo(RepositoryAction? action)
        {
            if (_sessionManager == null) return;
            _prevAction = _sessionManager.LastAction;
            _sessionManager.LastAction = (action ?? _prevAction ?? RepositoryAction.NONE);
        }

        private void RestoreLastAction()
        {
            SetCurrentActionTo(null);
        }
    }
}
