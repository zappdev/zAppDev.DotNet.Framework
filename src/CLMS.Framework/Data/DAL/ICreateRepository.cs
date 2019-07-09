namespace CLMS.Framework.Data.DAL
{
    public interface ICreateRepository: IRetrieveRepository
    {
        void SaveWithoutTransaction<T>(T entity) where T : class;

        void Save<T>(T entity) where T : class;

        void Insert<T>(T entity) where T : class;
    }
}
