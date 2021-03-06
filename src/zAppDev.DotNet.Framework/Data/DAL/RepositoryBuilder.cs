﻿// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Auditing;
using zAppDev.DotNet.Framework.Identity;
using zAppDev.DotNet.Framework.Workflow;

namespace zAppDev.DotNet.Framework.Data.DAL
{
    public class RepositoryBuilder : IRepositoryBuilder
    {


#if NETFRAMEWORK
        public ICreateRepository CreateCreateRepository(MiniSessionManager manager)
        {
            return null;
        }

        public IRetrieveRepository CreateRetrieveRepository(MiniSessionManager manager)
        {
            return null;
        }

        public IWorkflowRepository CreateWorkflowRepository(MiniSessionManager manager)
        {
            return null;
        }

        public IUpdateRepository CreateUpdateRepository(MiniSessionManager manager)
        {
            return null;
        }

        public IDeleteRepository CreateDeleteRepository(MiniSessionManager manager)
        {
            return null;
        }

        public IIdentityRepository CreateIdentityRepository(MiniSessionManager sessionManager) 
        {
            return null;
        }

        public IAuditingRepository CreateAuditingRepository(MiniSessionManager sessionManager)
        {
            return null;
        }
#else

        public ICreateRepository CreateCreateRepository(MiniSessionService manager)
        {
            return null;
        }

        public IRetrieveRepository CreateRetrieveRepository(MiniSessionService manager)
        {
            return null;
        }

        public IWorkflowRepository CreateWorkflowRepository(MiniSessionService manager)
        {
            return null;
        }

        public IUpdateRepository CreateUpdateRepository(MiniSessionService manager)
        {
            return null;
        }

        public IDeleteRepository CreateDeleteRepository(MiniSessionService manager)
        {
            return null;
        }

        public IIdentityRepository CreateIdentityRepository(MiniSessionService sessionManager)
        {
            return null;
        }

        public ICreateRepository CreateCreateRepository(IMiniSessionService manager)
        {
            return null;
        }

        public IRetrieveRepository CreateRetrieveRepository(IMiniSessionService manager)
        {
            return null;
        }

        public IWorkflowRepository CreateWorkflowRepository(IMiniSessionService manager)
        {
            return null;
        }

        public IUpdateRepository CreateUpdateRepository(IMiniSessionService manager)
        {
            return null;
        }

        public IDeleteRepository CreateDeleteRepository(IMiniSessionService manager)
        {
            return null;
        }

        public IIdentityRepository CreateIdentityRepository(IMiniSessionService sessionManager)
        {
            return null;
        }

        public IAuditingRepository CreateAuditingRepository(IMiniSessionService sessionManager)
        {
            return null;
        }
#endif


    }
}
