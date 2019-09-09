// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using zAppDev.DotNet.Framework.Data.DAL;

namespace zAppDev.DotNet.Framework.Workflow
{

    public interface IWorkflowStep
    {
        WorkflowStatus? Run();
        Guid? Id
        {
            get;
            set;
        }
    }
}