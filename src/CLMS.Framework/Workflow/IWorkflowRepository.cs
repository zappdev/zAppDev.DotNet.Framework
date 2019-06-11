namespace CLMS.Framework.Workflow
{
    public interface IWorkflowRepository
    {
        void DeleteWorkflowContextBase(IWorkflowContext workflowcontextbase, bool doNotCallDeleteForThis = false, bool isCascaded = false, object calledBy = null);
        void DeleteWorkflowSchedule(WorkflowSchedule workflowschedule, bool doNotCallDeleteForThis = false, bool isCascaded = false, object calledBy = null);
    }
}
