// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace zAppDev.DotNet.Framework.Middleware
{
    public class ApiMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiMiddleware(RequestDelegate next)
        {
            _next = next;
        } 

        public async Task Invoke(HttpContext context)
        {             
            await _next(context);
        }
    }
}
#endif