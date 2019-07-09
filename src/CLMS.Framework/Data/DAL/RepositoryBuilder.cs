using CLMS.Framework.Workflow;

namespace CLMS.Framework.Data.DAL
{
    public class RepositoryBuilder : IRepositoryBuilder
    {


#if NETFRAMEWORK
        public ICreateRepository CreateCreateRepository(MiniSessionManager manager)
        {
            return null;
        }

        public IRetrieveRepository CreateRetrieveRepository(MiniSessionManager manager)
        {
            return null;
        }

        public IWorkflowRepository CreateWorkflowRepository(MiniSessionManager manager)
        {
            return null;
        }

        public IUpdateRepository CreateUpdateRepository(MiniSessionManager manager)
        {
            return null;
        }

        public IDeleteRepository CreateDeleteRepository(MiniSessionManager manager)
        {
            return null;
        }

        public IIdentityRepository CreateIdentityRepository(MiniSessionManager sessionManager) 
        {
            return null;
        }
#else

        public ICreateRepository CreateCreateRepository(MiniSessionService manager)
        {
            return null;
        }

        public IRetrieveRepository CreateRetrieveRepository(MiniSessionService manager)
        {
            return null;
        }

        public IWorkflowRepository CreateWorkflowRepository(MiniSessionService manager)
        {
            return null;
        }

        public IUpdateRepository CreateUpdateRepository(MiniSessionService manager)
        {
            return null;
        }

        public IDeleteRepository CreateDeleteRepository(MiniSessionService manager)
        {
            return null;
        }

        public IIdentityRepository CreateIdentityRepository(MiniSessionService sessionManager)
        {
            return null;
        }

        public ICreateRepository CreateCreateRepository(IMiniSessionService manager)
        {
            return null;
        }

        public IRetrieveRepository CreateRetrieveRepository(IMiniSessionService manager)
        {
            return null;
        }

        public IWorkflowRepository CreateWorkflowRepository(IMiniSessionService manager)
        {
            return null;
        }

        public IUpdateRepository CreateUpdateRepository(IMiniSessionService manager)
        {
            return null;
        }

        public IDeleteRepository CreateDeleteRepository(IMiniSessionService manager)
        {
            return null;
        }

        public IIdentityRepository CreateIdentityRepository(IMiniSessionService sessionManager)
        {
            return null;
        }
#endif


    }
}
