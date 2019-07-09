using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLMS.Framework.Identity.Model;

namespace CLMS.Framework.Data.DAL
{
    public interface IDeleteRepository
    {
        void Delete<T>(T entity, bool isCascaded = false) where T : class;
    }
}
