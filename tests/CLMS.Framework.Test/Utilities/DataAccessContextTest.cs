using CLMS.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Utilities
{
    [TestClass]
    public class DataAccessContextTest
    {
        [TestMethod]
        public void InitTest()
        {
            var context = new DataAccessContext<object>
            {
                PageIndex = 0, PageSize = 10, Filter = null, SortByColumnName = null
            };
            
            Assert.AreEqual(0, context.PageIndex);
            Assert.AreEqual(10, context.PageSize);
            Assert.AreEqual(null, context.SortByColumnName);
            Assert.AreEqual(null, context.Filter);
        }
        
    }
}