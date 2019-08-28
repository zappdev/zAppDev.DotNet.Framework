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
