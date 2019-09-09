// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Mvc
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