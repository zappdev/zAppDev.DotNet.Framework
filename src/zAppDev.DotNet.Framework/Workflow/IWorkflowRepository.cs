// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
namespace zAppDev.DotNet.Framework.Workflow
{
    public interface IWorkflowRepository
    {
        void DeleteWorkflowContextBase(WorkflowContextBase workflowcontextbase, bool doNotCallDeleteForThis = false, bool isCascaded = false, object calledBy = null);
        void DeleteWorkflowSchedule(WorkflowSchedule workflowschedule, bool doNotCallDeleteForThis = false, bool isCascaded = false, object calledBy = null);
    }
}
