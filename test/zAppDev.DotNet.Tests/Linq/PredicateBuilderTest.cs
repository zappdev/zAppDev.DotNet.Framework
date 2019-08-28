using Microsoft.VisualStudio.TestTools.UnitTesting;
using zAppDev.DotNet.Framework.Linq;
using System.Collections.Generic;
using System.Linq;

namespace zAppDev.DotNet.Framework.Tests.Linq
{
    public class PredicateBuilderTest
    {
        [TestClass]
        public class SecurityDataDtosTest
        {
            [TestMethod()]
            public void PredicateTest()
            {
                var items = new List<int>();

                items.AddRange(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });

                var querable = items.AsQueryable();

                var size = querable.Where(PredicateBuilder.False<int>()).Count();
                Assert.AreEqual(0, size);

                size = querable.Where(PredicateBuilder.True<int>()).Count();
                Assert.AreEqual(10, size);

                size = querable.Where(PredicateBuilder.Or<int>((x) => x > 0, (x) => x > 10)).Count();
                Assert.AreEqual(10, size);

                size = querable.Where(PredicateBuilder.And<int>((x) => x > 5, (x) => x < 8)).Count();
                Assert.AreEqual(2, size);
            }
        }
    }
}
