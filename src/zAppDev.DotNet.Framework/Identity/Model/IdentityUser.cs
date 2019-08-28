using System.Security.Claims;
using System.Threading.Tasks;

#if NETFRAMEWORK
using Microsoft.AspNet.Identity;
#else
using Microsoft.AspNetCore.Identity;
#endif

namespace zAppDev.DotNet.Framework.Identity.Model
{
#if NETFRAMEWORK
    public class IdentityUser : IUser
#else
    public class IdentityUser : IdentityUser<string>
#endif
    {
        public IdentityUser(ApplicationUser user)
        {
            User = user;
        }

        public ApplicationUser User
        {
            get;
            set;
        }
#if NETFRAMEWORK
        public string UserName
#else
        public override string UserName
#endif
        {
            get
            {
                return User == null
                       ? string.Empty
                       : User.UserName;
            }
            set
            {
                if (User == null) return;
                User.UserName = value;
            }
        }

#if NETFRAMEWORK
        public string Id => User?.UserName;
#else
        public override string Id => User?.UserName;
#endif

        public string CurrentClientId
        {
            get;
            set;
        }

#if NETFRAMEWORK
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager manager, bool isPersistent = false)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            if (!string.IsNullOrEmpty(CurrentClientId))
            {
                userIdentity.AddClaim(new Claim("AspNet.Identity.ClientId", CurrentClientId));
            }
            userIdentity.SetIsPersistent(isPersistent);
            // Add custom user claims here
            return userIdentity;
        }
#endif
    }
}