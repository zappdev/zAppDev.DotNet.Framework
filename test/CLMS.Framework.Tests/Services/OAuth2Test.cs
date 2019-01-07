using Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Services
{
    [TestClass]
    public class OAuth2Test
    {
        [TestMethod]
        public void InitTest()
        {
            var token = new OAuth2TokenData();
            
            token.Parse("{'access_token': 'ALM'}");
            
            Assert.AreEqual("ALM", token.Token);

            var auth2ReturnUrl = new OAuth2ReturnUrl("www.clms.gr");
            Assert.AreEqual("www.clms.gr", auth2ReturnUrl.ReturnUrl);

            var code = new OAuth2Code("ABS");
            Assert.AreEqual("ABS", code.Code);
        }
    }
}