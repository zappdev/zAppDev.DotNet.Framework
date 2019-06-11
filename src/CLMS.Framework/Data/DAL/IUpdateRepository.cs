namespace CLMS.Framework.Data.DAL
{
    public interface IUpdateRepository
    {
        void Update<T>(T entity) where T : class;
    }
}
