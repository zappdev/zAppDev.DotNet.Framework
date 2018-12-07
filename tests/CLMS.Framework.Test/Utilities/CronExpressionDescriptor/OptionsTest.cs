using Microsoft.VisualStudio.TestTools.UnitTesting;
using CLMS.Framework.Utilities.CronExpressionDescriptor;

namespace CLMS.Framework.Test.Utilities.CronExpressionDescriptor
{
    [TestClass]
    public class OptionsTest
    {
        [TestMethod]
        public void UseClassTest()
        {
            var option = new Options();

            option.Verbose = true;
            option.Use24HourTimeFormat = true;
            option.ThrowExceptionOnParseError = false;
            option.Locale = "el";
            option.DayOfWeekStartIndexZero = true;

            Assert.AreEqual(option.Locale, "el");
            Assert.AreEqual(option.ThrowExceptionOnParseError, false);
            Assert.AreEqual(option.Use24HourTimeFormat, true);
            Assert.AreEqual(option.Verbose, true);
            Assert.AreEqual(option.DayOfWeekStartIndexZero, true);
        }
    }
}
