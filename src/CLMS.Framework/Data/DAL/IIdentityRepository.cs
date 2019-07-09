using CLMS.Framework.Identity.Model;

namespace CLMS.Framework.Data.DAL
{
    public interface IIdentityRepository: ICreateRepository
    {
        void DeleteApplicationUser(ApplicationUser applicationuser, bool doNotCallDeleteForThis = false, bool isCascaded = false, object calledBy = null);
    }
}
