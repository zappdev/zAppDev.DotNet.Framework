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