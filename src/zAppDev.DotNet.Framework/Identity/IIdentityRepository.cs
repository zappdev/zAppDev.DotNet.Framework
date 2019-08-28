using zAppDev.DotNet.Framework.Data.DAL;
using zAppDev.DotNet.Framework.Identity.Model;

namespace zAppDev.DotNet.Framework.Identity
{
    public interface IIdentityRepository: ICreateRepository
    {
        void DeleteApplicationUser(ApplicationUser applicationuser, bool doNotCallDeleteForThis = false, bool isCascaded = false, object calledBy = null);
    }
}
