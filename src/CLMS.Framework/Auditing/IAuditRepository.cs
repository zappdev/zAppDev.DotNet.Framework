using CLMS.Framework.Auditing.Model;
using CLMS.Framework.Data.DAL;

namespace CLMS.Framework.Auditing
{
    public interface IAuditingRepository : ICreateRepository
    {
        void DeleteAuditPropertyConfiguration(AuditPropertyConfiguration propertyConfiguration, bool doNotCallDeleteForThis = false, bool isCascaded = false, object calledBy = null);
    }
}
