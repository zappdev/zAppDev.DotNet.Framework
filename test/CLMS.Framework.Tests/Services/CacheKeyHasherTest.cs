using CLMS.Framework.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Services
{
    [TestClass]
    public class CacheKeyHasherTest
    {
        [TestMethod]
        public void PropertyTest() 
        {
            var hasher = new CacheKeyHasher{
                ApiName = "Api",
                UserName = "theofilis",
                Operation = "AddItem",
                OriginalKey = "1"
            };

            Assert.AreEqual("Api|theofilis|AddItem|c4ca4238a0b923820dcc509a6f75849b", hasher.GetHashedKey());

        }
    }
}
