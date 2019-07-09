using System;
using CLMS.Framework.Identity.Model;

namespace CLMS.Framework.Hubs
{
    public interface IAccountHub
    {
        void RaiseSignOutEvent(string UserName, DateTime? Time, string _groupName = null);
        void RaiseSignInEvent(string UserName, DateTime? Time, string _groupName = null);
        void RaiseExternalUserCreatingEvent(ApplicationUser user, string _groupName = null);
    }
}
