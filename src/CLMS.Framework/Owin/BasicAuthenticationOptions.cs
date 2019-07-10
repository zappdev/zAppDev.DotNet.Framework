#if NETFRAMEWORK
using Microsoft.Owin.Security;

namespace CLMS.Framework.Owin
{
    public class BasicAuthenticationOptions : AuthenticationOptions
    {
        public BasicAuthenticationMiddleware.CredentialValidationFunction CredentialValidationFunction
        {
            get;
            private set;
        }
        public string Realm
        {
            get;
        }

        public BasicAuthenticationOptions(string realm, BasicAuthenticationMiddleware.CredentialValidationFunction validationFunction)
        : base("Basic")
        {
            Realm = realm;
            CredentialValidationFunction = validationFunction;
        }
    }
}
#else
#endif