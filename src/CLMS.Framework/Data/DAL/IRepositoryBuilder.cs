using CLMS.Framework.Workflow;

namespace CLMS.Framework.Data.DAL
{
    public interface IRepositoryBuilder
    {
        IRetrieveRepository CreateRetrieveRepository();
        IWorkflowRepository CreateWorkflowRepository();

        IUpdateRepository CreateUpdateRepository();

#if NETFRAMEWORK
        ICreateRepository CreateCreateRepository(MiniSessionManager manager);
#else
        ICreateRepository CreateCreateRepository(MiniSessionService manager);
#endif

        IDeleteRepository CreateDeleteRepository();
    }
}