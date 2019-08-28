using Microsoft.VisualStudio.TestTools.UnitTesting;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Tests.Utilities
{
    [TestClass]
    public class ExtensionMethodsTest
    {
        [TestMethod]
        public void SplitTest()
        {
            Assert.AreEqual(2, "a,b".Split(new[]{ ","}, true).Length);
            Assert.AreEqual(2, "a,".Split(new[]{ ","}, false).Length);
            Assert.AreEqual(2, "a,b".Split(new[]{ ','}, true).Length);
            Assert.AreEqual(2, "a,".Split(new[]{ ','}, false).Length);
            
            
            Assert.AreEqual(2, "a,b".SplitExtended(new[]{ ","}).Length);
            Assert.AreEqual(2, "a,b".SplitExtended(',').Length);
            Assert.AreEqual(2, "a,b".SplitExtended((char?)',').Length);
            
            Assert.AreEqual(2, "a,b".SplitExtended(new[]{ ','}).Length);
            Assert.AreEqual(2, "a,b".SplitExtended(new char?[]{ ','}).Length);
            Assert.AreEqual(2, "a,b".SplitExtended(new char?[]{ ','}, true).Length);
            
            Assert.AreEqual("a", ((char?)'a').ToStringArray()[0]);
        }
        
    }
}