﻿// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Auditing;
using zAppDev.DotNet.Framework.Identity;
using zAppDev.DotNet.Framework.Workflow;

namespace zAppDev.DotNet.Framework.Data.DAL
{
    public interface IRepositoryBuilder
    {

#if NETFRAMEWORK
        IRetrieveRepository CreateRetrieveRepository(MiniSessionManager manager=null);
        IWorkflowRepository CreateWorkflowRepository(MiniSessionManager manager = null);

        IUpdateRepository CreateUpdateRepository(MiniSessionManager manager = null);
        ICreateRepository CreateCreateRepository(MiniSessionManager manager = null);
        IDeleteRepository CreateDeleteRepository(MiniSessionManager manager = null);
        IIdentityRepository CreateIdentityRepository(MiniSessionManager sessionManager);
        IAuditingRepository CreateAuditingRepository(MiniSessionManager sessionManager = null);
#else
        IRetrieveRepository CreateRetrieveRepository(MiniSessionService manager = null);
        IWorkflowRepository CreateWorkflowRepository(MiniSessionService manager = null);
        IUpdateRepository CreateUpdateRepository(MiniSessionService manager = null);
        ICreateRepository CreateCreateRepository(MiniSessionService manager = null);
        IDeleteRepository CreateDeleteRepository(MiniSessionService manager = null);
        IIdentityRepository CreateIdentityRepository(MiniSessionService sessionManager = null);
        
        IRetrieveRepository CreateRetrieveRepository(IMiniSessionService manager);
        IWorkflowRepository CreateWorkflowRepository(IMiniSessionService manager);
        IUpdateRepository CreateUpdateRepository(IMiniSessionService manager);
        ICreateRepository CreateCreateRepository(IMiniSessionService sessionManager);
        IDeleteRepository CreateDeleteRepository(IMiniSessionService manager);
        IIdentityRepository CreateIdentityRepository(IMiniSessionService sessionManager);
        IAuditingRepository CreateAuditingRepository(IMiniSessionService sessionManager = null);
#endif


    }
}