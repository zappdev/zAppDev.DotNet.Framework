using System;
using CLMS.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Utilities
{
    [TestClass]
    public class CommonTest
    {
        [TestMethod]
        public void SafeCastTest()
        {
            Assert.AreEqual(1, Common.SafeCast<int>((object)1));
            Assert.AreEqual(1, Common.SafeCast<int>((object)"1"));
            Assert.AreEqual(1, Common.SafeCast<int?>((object)"1"));

            Assert.AreEqual(1.2, Common.SafeCast<double>((object)1.2));
            Assert.AreEqual(1.2, Common.SafeCast<double>((object)"1,2"));
            Assert.AreEqual(1.2, Common.SafeCast<double?>((object)"1,2"));

            Assert.IsTrue(Math.Abs(Common.SafeCast<float>((object)1.2) - 1.2) < 0.0001);
            Assert.IsTrue(Math.Abs(Common.SafeCast<float>((object)"1,2") - 1.2) < 0.0001);
            Assert.IsTrue(Math.Abs(Common.SafeCast<float?>((object)"1,2").Value - 1.2) < 0.0001);

            Assert.AreEqual(1, Common.SafeCast<short>((object)1));
            Assert.AreEqual(1, Common.SafeCast<short>((object)"1"));
            Assert.AreEqual((short)1, Common.SafeCast<short?>("1"));

            Assert.AreEqual(1, Common.SafeCast<long>((object)1));
            Assert.AreEqual(1, Common.SafeCast<long>((object)"1"));
            Assert.AreEqual(1, Common.SafeCast<long?>((object)"1"));

            Assert.AreEqual(1, Common.SafeCast<decimal>((object)1));
            Assert.AreEqual(1, Common.SafeCast<decimal>((object)"1"));
            Assert.AreEqual((decimal)1, Common.SafeCast<decimal?>((object)"1"));

            Assert.AreEqual(true, Common.SafeCast<bool>((object)true));
            Assert.AreEqual(true, Common.SafeCast<bool>((object)"true"));
            Assert.AreEqual(true, Common.SafeCast<bool?>((object)"true"));
        }
    }
}
