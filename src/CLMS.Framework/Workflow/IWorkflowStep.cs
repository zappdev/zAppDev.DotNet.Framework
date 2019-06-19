using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CLMS.Framework.Data.DAL;

namespace CLMS.Framework.Workflow
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