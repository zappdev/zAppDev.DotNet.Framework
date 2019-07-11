using System.Collections.Generic;

namespace CLMS.Framework.Mvc
{
    public interface IDTOHelper
    {
        T GetDTOFromModel<T>(object original, int level = 0, int maxLevel = 4) where T : class;
        T GetModelFromDTO<T>(IViewModelDTO<T> dtoInstance, int level = 0, int maxLevel = 4) where T : class;
        object GetClientKey(object instance, object keyValue);
        T GetSeenModelInstance<T>(object clientKey, string type, List<string> baseClasses = null) where T : class;
        void UpdateSeenModelInstances(object dto, object original);
    }
}