using System;

namespace CLMS.Framework.Hubs
{
    public interface IApplicationHub: IAccountHub
    {
        void RaiseApplicationStartEvent(string _groupName = null);
        void RaiseApplicationEndEvent(string _groupName = null);
        void RaiseApplicationErrorEvent(Exception exception, string _groupName = null);
        void RaiseSessionStartEvent(string _groupName = null);
        void RaiseOnInstanceSaveEvent(object Instance, string _groupName = null);
        void RaiseFileDownloadEvent(string path, string username, string _groupName = null);
    }
}
