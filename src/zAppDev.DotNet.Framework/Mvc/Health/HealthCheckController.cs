#if NETFRAMEWORK
#else
using System;
using zAppDev.DotNet.Framework.Data;
using zAppDev.DotNet.Framework.Utilities;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace zAppDev.DotNet.Framework.Mvc.Health
{
    [Route("api/_health")]
    [ApiController]
    [SwaggerTag("Return API Health")]
    public class HealthCheckController : ControllerBase
    {
        protected ServiceLocator ServiceLocator
        {
            get;
            set;
        }

        public HealthCheckController(IServiceProvider serviceProvider)
        {
            ServiceLocator = new ServiceLocator(serviceProvider);
            ServiceLocator.SetLocatorProvider(serviceProvider);
        }

        [Route("check")]
        [HttpGet]
        public HealthCheckResult Check()
        {
            // TODO - do additional checks here, besides checking the db
            return MiniSessionManager.ExecuteInUoW(manager => new HealthCheckResult
            {
                Status = "ok"
            });
        }

        public class HealthCheckResult
        {
            public string Status
            {
                get;
                set;
            }
        }
    }
}
#endif