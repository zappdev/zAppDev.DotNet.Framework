using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CLMS.Framework.Data.DAL;

namespace CLMS.Framework.Workflow
{
    public abstract class WorkflowImplementation
    {
        public static IRepositoryBuilder Builder;

        public abstract WorkflowStatus? Execute();
        public abstract WorkflowStatus? Continue(string stepName);
        public abstract T GetStep<T>() where T : class;
        public abstract IWorkflowContext GetContext();
        public abstract void RestoreState(IWorkflowContext wfBase);
        public abstract void CreatePendingJob(string stepName);

        public abstract WorkflowStatus? Status
        {
            get;
            set;
        }

        public static void RemovePendingJob(Guid id)
        {
            var instance = Builder.CreateRetrieveRepository().GetById<IWorkflowContext>(id);
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

    public interface IWorkflowStep
    {
        WorkflowStatus? Run();
        Guid? Id
        {
            get;
            set;
        }
    }

    public class WorkflowManager
    {
        public static IRepositoryBuilder Builder;

        private static readonly Lazy<WorkflowManager> Lazy =
            new Lazy<WorkflowManager>(() => new WorkflowManager());

        public static WorkflowManager Current => Lazy.Value;


        private List<Type> _workflowImplementationTypes = new List<Type>();

        public void Init(Assembly workflowAssembly)
        {
            _workflowImplementationTypes = workflowAssembly.GetTypes().Where(t => t.BaseType == typeof(WorkflowImplementation)).ToList();
        }

        public IEnumerable<IWorkflowContext> GetAllPendingJobs(int pageSize, int currentPage)
        {
            return Builder.CreateRetrieveRepository().GetAll<IWorkflowContext>();
        }

        public IWorkflowExecutionResult ExecuteWorkflow(string wfName)
        {
            var wf = GetWorkflowInstance(wfName + "Workflow");
            try
            {
                if (wf == null)
                {
                    throw new ApplicationException($"Workflow '{wfName}' not Found!");
                }
                wf.Execute();
                return new WorkflowExecutionResult { Context = wf.GetContext(), Status = wf.Status };
            }
            catch(Exception e)
            {
                var context = wf == null
                              ? null
                              : wf.GetContext();
                log4net.LogManager.GetLogger(GetType()).Error($"Error executing Workflow '{wfName}'!", e);
                return new WorkflowExecutionResult { Context = context, Status = WorkflowStatus.Failed };
            }
        }

        private WorkflowImplementation GetWorkflowInstance(IWorkflowContext pendingJob)
        {
            return GetWorkflowInstance(pendingJob.Name);
        }

        private WorkflowImplementation GetWorkflowInstance(string wfName)
        {
            try
            {
                var typeofWf = _workflowImplementationTypes.FirstOrDefault(t => t.Name == wfName);
                if (typeofWf == null)
                {
                    return null;
                }
                var wf = Activator.CreateInstance(typeofWf) as WorkflowImplementation;
                return wf;
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(GetType()).Error($"Error getting Workflow Instance '{wfName}'!", e);
                return null;
            }
        }

        public IWorkflowExecutionResult Continue(Guid? id)
        {
            var pendingWorkflow = Builder.CreateRetrieveRepository().GetById<IWorkflowContext>(id);
            try
            {
                return Continue(pendingWorkflow);
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(GetType()).Error($"Error continuing pending Workflow with Id: '{id}' !", e);
                return new WorkflowExecutionResult { Context = pendingWorkflow, Status = WorkflowStatus.Failed };
            }
        }

        public IWorkflowExecutionResult Continue(IWorkflowContext pendingWorkflow)
        {
            try
            {
                var wf = GetWorkflowInstance(pendingWorkflow);
                //restore state
                wf.RestoreState(pendingWorkflow);
                //Continue
                var status = wf.Continue(pendingWorkflow.PendingStep);
                if (status == WorkflowStatus.Completed)
                {
                    //remove from list
                    RemovePendingJob(pendingWorkflow);
                }
                return new WorkflowExecutionResult { Context = pendingWorkflow, Status = status };
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(GetType()).Error($"Error continuing pending Workflow '{pendingWorkflow?.Name}' !", e);
                return new WorkflowExecutionResult { Context = pendingWorkflow, Status = WorkflowStatus.Failed };
            }
        }

        public IWorkflowExecutionResult Cancel(Guid id)
        {
            var pendingWorkflow = Builder.CreateRetrieveRepository().GetById<IWorkflowContext>(id);
            try
            {
                return Cancel(pendingWorkflow);
            }
            catch(Exception e)
            {
                log4net.LogManager.GetLogger(GetType()).Error($"Error cancelling pending Workflow with Id: '{id}' !", e);
                return new WorkflowExecutionResult { Context = pendingWorkflow, Status = WorkflowStatus.Failed };
            }
        }

        public IWorkflowExecutionResult Cancel(IWorkflowContext pendingWorkflow)
        {
            try
            {
                Builder.CreateWorkflowRepository().DeleteWorkflowContextBase(pendingWorkflow);
                return new WorkflowExecutionResult { Context = pendingWorkflow, Status = WorkflowStatus.Cancelled };
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(GetType()).Error($"Error cancelling pending Workflow '{pendingWorkflow?.Name}' !", e);
                return new WorkflowExecutionResult { Context = pendingWorkflow, Status = WorkflowStatus.Failed };
            }
        }

        private IWorkflowExecutionResult RemovePendingJob(IWorkflowContext pendingWorkflow)
        {
            try
            {
                Builder.CreateWorkflowRepository().DeleteWorkflowContextBase(pendingWorkflow);
                return new WorkflowExecutionResult { Context = pendingWorkflow, Status = WorkflowStatus.Completed };
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(GetType()).Error($"Error removing pending Workflow '{pendingWorkflow?.Name}' !", e);
                return new WorkflowExecutionResult { Context = pendingWorkflow, Status = WorkflowStatus.Failed };
            }
        }

        public IWorkflowExecutionResult Expire(Guid value)
        {
            throw new NotImplementedException();
        }
    }
}