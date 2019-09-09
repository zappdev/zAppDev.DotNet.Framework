// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Powershell
{
    public class GenericInvocationResult<T>
    {
        public bool Successful { get; set; }
    }

    public class InvocationResult<T> : GenericInvocationResult<T>
    {        
        public T Result { get; set; }
    }

    public class InvocationResults<T> : GenericInvocationResult<T>
    {
        public List<T> Result { get; set; }
    }
}
