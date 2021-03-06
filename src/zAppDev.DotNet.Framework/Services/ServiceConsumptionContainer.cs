﻿// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System.Net.Http;

namespace zAppDev.DotNet.Framework.Services
{
    /// <summary>
    /// A container that holds any items needed to be remembered while a Remote Service is being executed
    /// This class does not hold the actual RESULT of a service invokation; it keeps any additional information that might be useful, 
    /// regarding the actual invokation
    /// </summary>
    public class ServiceConsumptionContainer
    {
        public HttpResponseMessage HttpResponseMessage { get; set; }
    }
}
