using zAppDev.DotNet.Framework.Auditing.Model;
using zAppDev.DotNet.Framework.Data.DAL;

namespace zAppDev.DotNet.Framework.Auditing
{
    public interface IAuditingRepository : ICreateRepository
    {
        void DeleteAuditPropertyConfiguration(AuditPropertyConfiguration propertyConfiguration, 
            bool doNotCallDeleteForThis = false, 
            bool isCascaded = false, 
            object calledBy = null);

        void DeleteAuditEntityConfiguration(
            AuditEntityConfiguration auditentityconfiguration,
            bool doNotCallDeleteForThis = false, bool isCascaded = false, object calledBy = null);
    }
}
