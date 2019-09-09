// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using zAppDev.DotNet.Framework.Identity.Model;

namespace zAppDev.DotNet.Framework.Hubs
{
    public interface IAccountHub
    {
        void RaiseSignOutEvent(string UserName, DateTime? Time, string _groupName = null);
        void RaiseSignInEvent(string UserName, DateTime? Time, string _groupName = null);
        void RaiseExternalUserCreatingEvent(ApplicationUser user, string _groupName = null);
        List<string> GetAllConnectedUsers();
        bool IsUserConnected(string username);
    }
}
