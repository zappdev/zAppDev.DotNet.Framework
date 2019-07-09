using CLMS.Framework.Workflow;

namespace CLMS.Framework.Data.DAL
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
#endif


    }
}