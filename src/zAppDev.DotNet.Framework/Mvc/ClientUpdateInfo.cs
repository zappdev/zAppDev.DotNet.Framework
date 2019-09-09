// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;

namespace zAppDev.DotNet.Framework.Mvc
{
    public class ClientUpdateInfo
    {
        public object Instance
        {
            get;
            set;
        }
        public Func<object, object> DtoConverter
        {
            get;
            set;
        }
        public int Order
        {
            get;
            set;
        }
    }

    
}
