using CLMS.Framework.Identity.Model;
using System;
using System.Linq;

namespace CLMS.Framework.Identity
{
    public static class ClaimsIdentityExtensions
    {
        public static bool GetIsPersistent(this System.Security.Claims.ClaimsIdentity identity)
        {
            return identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.PersistentLogin) != null;
        }

        public static void SetIsPersistent(this System.Security.Claims.ClaimsIdentity identity, bool isPersistent)
        {
            var claim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.PersistentLogin);
            if (isPersistent)
            {
                if (claim == null)
                {
                    identity.AddClaim(new System.Security.Claims.Claim(ClaimTypes.PersistentLogin, bool.TrueString));
                }
            }
            else if (claim != null)
            {
                identity.RemoveClaim(claim);
            }
        }
    }
}