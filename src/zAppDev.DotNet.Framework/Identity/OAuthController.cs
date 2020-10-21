#if NETFRAMEWORK
#else
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using zAppDev.DotNet.Framework.Data;
using Swashbuckle.AspNetCore.Annotations;
using zAppDev.DotNet.Framework.Utilities;


namespace zAppDev.DotNet.Framework.Identity
{
    public class AuthRequest
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }

    [Route("OAuth")]
    [ApiController]
    [SwaggerTag("Return JWT Tokens")]
    public class OAuthController : ControllerBase
    {
        private IMiniSessionService _manager;
        private IConfiguration _configuration;

        private string _jwtKey;
        private DateTime _jwtExpirationTime;

        public OAuthController(IServiceProvider serviceProvider, IConfiguration Configuration, IMiniSessionService manager)
        {
            _manager = manager;
            _configuration = Configuration;

            _jwtKey = _configuration.GetValue("configuration:appSettings:add:JWTKey:value",
                "MIksRlTn0KG6nmjW*fzq*FYTY0RifkNQE%QTqdfS81CgNEGtUmMCY5XEgPTSL&28");
            _jwtExpirationTime = DateTime.UtcNow.AddSeconds(_configuration.GetValue("configuration:appSettings:add:JWTExpirationTime:value", 7200));

            ServiceLocator.SetLocatorProvider(serviceProvider);
        }

        [Route("Token")]
        [HttpPost]
        public IActionResult Token(AuthRequest authRequest)
        {
            using (var session = _manager.OpenSession())
            {
                var success = IdentityHelper.SignIn(authRequest.Username, authRequest.Password, false);

                ActionResult response = new EmptyResult();

                if (success == true)
                {
                    var tokenInfo = IdentityHelper.GenerateToken(authRequest.Username, _jwtKey, _jwtExpirationTime);
                    var result = new
                    {
                        idToken = tokenInfo.Token,
                        expiresIn = tokenInfo.ExpiresIn,
                    };
                    response = Ok(result);
                }
                _manager.CloseSession();
                return response;
            }
        }
    }
}
#endif