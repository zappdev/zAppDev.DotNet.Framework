using System;
using CLMS.Framework.Data.DAL;

namespace CLMS.Framework.Workflow
{
    public abstract class WorkflowImplementation
    {
        public static IRepositoryBuilder Builder;

        public abstract WorkflowStatus? Execute();
        public abstract WorkflowStatus? Continue(string stepName);
        public abstract T GetStep<T>() where T : class;
        public abstract WorkflowContextBase GetContext();
        public abstract void RestoreState(WorkflowContextBase wfBase);
        public abstract void CreatePendingJob(string stepName);

        public abstract WorkflowStatus? Status
        {
            get;
            set;
        }

        public static void RemovePendingJob(Guid id)
        {
            var instance = Builder.CreateRetrieveRepository().GetById<WorkflowContextBase>(id);
            Builder.CreateWorkflowRepository().DeleteWorkflowContextBase(instance);
        }

        public static void GetAllPending<T>()
        {
            Builder.CreateRetrieveRepository().GetAll<T>();
        }

        public static T GetPendingByKey<T>(Guid id) where T : class
        {
            return Builder.CreateRetrieveRepository().GetById<T>(id);
        }
    }
}