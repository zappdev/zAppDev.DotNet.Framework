using CLMS.Framework.Utilities.CronExpressionDescriptor;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Utilities.CronExpressionDescriptor
{
    [TestClass]
    public class ExpressionDescriptorTest {

        [TestMethod]
        public void InitTest()
        {
            var obj = new ExpressionDescriptor("*/5 1 */5 */5 */5");

            Assert.AreEqual("Every 5 minutes, between 01:00 AM and 01:59 AM, every 5 days, every 5 days of the week, every 5 months", obj.GetDescription(DescriptionTypeEnum.FULL));
            
            Assert.AreEqual("Every 5 minutes, between 01:00 AM and 01:59 AM", obj.GetDescription(DescriptionTypeEnum.TIMEOFDAY));

            Assert.AreEqual("Between 01:00 AM and 01:59 AM", obj.GetDescription(DescriptionTypeEnum.HOURS));
            Assert.AreEqual("Every 5 minutes", obj.GetDescription(DescriptionTypeEnum.MINUTES));
            //Assert.AreEqual("", obj.GetDescription(DescriptionTypeEnum.SECONDS));
            Assert.AreEqual(", every 5 days", obj.GetDescription(DescriptionTypeEnum.DAYOFMONTH));
            Assert.AreEqual(", every 5 months", obj.GetDescription(DescriptionTypeEnum.MONTH));

            Assert.AreEqual(", every 5 days of the week", obj.GetDescription(DescriptionTypeEnum.DAYOFWEEK));
            //Assert.AreEqual("", obj.GetDescription(DescriptionTypeEnum.YEAR));
        }

        [TestMethod]
        public void GetDescriptionTest()
        {
            Assert.AreEqual("*", ExpressionDescriptor.GetDescription("*"));
            Assert.AreEqual("Every second", ExpressionDescriptor.GetDescription("* * * * * *"));
            Assert.AreEqual("Every minute", ExpressionDescriptor.GetDescription("* * * * *"));
            Assert.AreEqual("Every minute", ExpressionDescriptor.GetDescription("* * * * *", new Options()));
        }
    }
}
