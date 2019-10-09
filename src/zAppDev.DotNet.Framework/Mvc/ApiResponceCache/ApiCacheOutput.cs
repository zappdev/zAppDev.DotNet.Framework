#if NETFRAMEWORK
#else
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Mvc.API
{
    public class ApiCacheOutput
    {
        public ApiCacheOutput(byte[] body, IHeaderDictionary headers)
        {
            Body = body;
            Headers = new Dictionary<string, string>();

            foreach (string name in headers.Keys)
            {
                Headers.Add(name, headers[name]);
            }
        }

        public byte[] Body { get; set; }

        public Dictionary<string, string> Headers { get; }
    }
}
#endif