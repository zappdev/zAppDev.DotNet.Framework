using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Data.Domain
{
    public interface IDirtyTracker
    {
        void StartTracking();
        void StopTracking();
        void ClearDirty();
        bool IsDirty();
        bool IsPropertyDirty(string propName);
        Dictionary<string, bool> GetDirtyProperties();
    }
}
