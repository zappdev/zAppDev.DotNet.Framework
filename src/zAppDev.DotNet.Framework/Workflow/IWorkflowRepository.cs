namespace zAppDev.DotNet.Framework.Workflow
{
    public interface IWorkflowRepository
    {
        void DeleteWorkflowContextBase(WorkflowContextBase workflowcontextbase, bool doNotCallDeleteForThis = false, bool isCascaded = false, object calledBy = null);
        void DeleteWorkflowSchedule(WorkflowSchedule workflowschedule, bool doNotCallDeleteForThis = false, bool isCascaded = false, object calledBy = null);
    }
}
