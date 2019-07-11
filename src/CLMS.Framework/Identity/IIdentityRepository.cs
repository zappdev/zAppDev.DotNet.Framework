using CLMS.Framework.Data.DAL;
using CLMS.Framework.Identity.Model;

namespace CLMS.Framework.Identity
{
    public interface IIdentityRepository: ICreateRepository
    {
        void DeleteApplicationUser(ApplicationUser applicationuser, bool doNotCallDeleteForThis = false, bool isCascaded = false, object calledBy = null);
    }
}
