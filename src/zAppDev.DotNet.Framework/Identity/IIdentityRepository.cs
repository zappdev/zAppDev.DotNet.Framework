// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Data.DAL;
using zAppDev.DotNet.Framework.Identity.Model;

namespace zAppDev.DotNet.Framework.Identity
{
    public interface IIdentityRepository: ICreateRepository
    {
        void DeleteApplicationUser(ApplicationUser applicationuser, bool doNotCallDeleteForThis = false, bool isCascaded = false, object calledBy = null);
    }
}
