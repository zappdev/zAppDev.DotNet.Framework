// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Identity.Model;
using System;
using System.Linq;

namespace zAppDev.DotNet.Framework.Identity
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