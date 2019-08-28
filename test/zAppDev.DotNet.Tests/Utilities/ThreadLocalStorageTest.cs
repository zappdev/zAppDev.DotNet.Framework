using zAppDev.DotNet.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Utilities
{
    [TestClass]
    public class ThreadLocalStorageTest
    {
        [TestMethod]
        public void SetTest()
        {
            ThreadLocalStorage.Set("Count", 10);
            Assert.AreEqual(10, ThreadLocalStorage.Get<int>("Count"));

            ThreadLocalStorage.Set("Count", 20);
            Assert.AreEqual(20, ThreadLocalStorage.Get<int>("Count"));
        }

        [TestMethod]
        public void RemoveTest()
        {
            ThreadLocalStorage.Remove("Count");
            Assert.AreEqual(0, ThreadLocalStorage.Get<int>("Count"));
        }
    }
}
