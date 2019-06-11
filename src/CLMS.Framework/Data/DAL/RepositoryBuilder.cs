using CLMS.Framework.Workflow;

namespace CLMS.Framework.Data.DAL
{
    public class RepositoryBuilder : IRepositoryBuilder
    {
        public IRetrieveRepository CreateRetrieveRepository()
        {
            return null;
        }

        public IWorkflowRepository CreateWorkflowRepository()
        {
            return null;
        }

        public IUpdateRepository CreateUpdateRepository()
        {
            return null;
        }


#if NETFRAMEWORK
        public ICreateRepository CreateCreateRepository(MiniSessionManager manager)
        {
            return null;
        }
#else
        public ICreateRepository CreateCreateRepository(MiniSessionService manager)
        {
            return null;
        }
#endif

        public IDeleteRepository CreateDeleteRepository()
        {
            return null;
        }
    }
}
