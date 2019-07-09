using CLMS.Framework.Identity.Model;

namespace CLMS.Framework.Data.DAL
{
    public interface IIdentityRepository: ICreateRepository
    {
        void DeleteApplicationUser(ApplicationUser user);
    }
}
