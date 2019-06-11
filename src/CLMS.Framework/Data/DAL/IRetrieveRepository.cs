using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CLMS.Framework.Data.DAL
{
    public interface IRetrieveRepository
    {
        T GetById<T>(object id, bool throwIfNotFound = true) where T : class;
        List<T> Get<T>(Expression<Func<T, bool>> predicate, bool cacheQuery = true);
        List<T> Get<T>(Expression<Func<T, bool>> predicate,
                       int startRowIndex,
                       int pageSize,
                       Dictionary<Expression<Func<T, object>>, bool> orderBy,
                       out int totalRecords, bool cacheQuery = true);
        List<T> GetAll<T>(bool cacheQuery = true);
        List<T> GetAll<T>(int startRowIndex, int pageSize, out int totalRecords, bool cacheQuery = true);
        IQueryable<T> GetAsQueryable<T>(Expression<Func<T, bool>> predicate = null, bool cacheQuery = true);
        IQueryable<T> GetMainQuery<T>();
    }
}
