using System.Collections.Generic;
using zAppDev.DotNet.Framework.Powershell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Powershell
{
    
    [TestClass]
    public class GenericInvocationResultTest
    {
        [TestMethod]
        public void InitTest()
        {
            var obj = new GenericInvocationResult<object>();               
            Assert.IsFalse(obj.Successful);

            obj.Successful = true;
            Assert.IsTrue(obj.Successful);
        }
    }
    
    [TestClass]
    public class InvocationResultTest
    {
        [TestMethod]
        public void InitTest()
        {
            var obj = new InvocationResult<object>();               
            Assert.IsNull(obj.Result);
            Assert.IsFalse(obj.Successful);
            
            obj.Successful = true;
            Assert.IsTrue(obj.Successful);

            obj.Result = 1;
            Assert.AreEqual(1, obj.Result);
        }
    }
    
    [TestClass]
    public class InvocationResultsTest
    {
        [TestMethod]
        public void InitTest()
        {
            var obj = new InvocationResults<object>();               
            Assert.IsNull(obj.Result);
            Assert.IsFalse(obj.Successful);
            
            obj.Successful = true;
            Assert.IsTrue(obj.Successful);
            
            obj.Result = new List<object>{ 1 };
            Assert.AreEqual(1, obj.Result.Count);
        }
    }
}
