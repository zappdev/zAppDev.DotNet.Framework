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
