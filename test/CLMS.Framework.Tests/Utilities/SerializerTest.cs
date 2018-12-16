using CLMS.Framework.SecurityDataDtos;
using CLMS.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace CLMS.Framework.Tests.Utilities
{
    [TestClass]
    public class SerializerTest
    {
        [TestMethod]
        public void SerializeTest()
        {
            var operation = new ApplicationPermission
            {
                Id = 0,
                IsCustom = true,
                Name = "name"
            };

            var serializer = new Serializer<ApplicationPermission>();

            Assert.AreEqual("9d28e27cf9b133fe544dd4814f353cb7", Common.GetMD5Hash(serializer.ToJson(operation, true)));
            Assert.AreEqual("9ba3c150bf0668ae3456b30f8f37fa77", Common.GetMD5Hash(serializer.ToXml(operation, true)));

            var jsonStr = serializer.ToJson(operation);
            var xmlStr = serializer.ToXml(operation);

            Assert.AreEqual(operation.Name, serializer.FromJson(jsonStr).Name);
            Assert.AreEqual(operation.Name, serializer.FromXml(xmlStr).Name);

            var enumSerializer = new Serializer<AppDevSemantic>();

            Assert.AreEqual(AppDevSemantic.CalculatedExpression, enumSerializer.ParseEnum("CalculatedExpression"));
            Assert.AreEqual(AppDevSemantic.None, enumSerializer.ParseEnum(null));

            var ioStream = new Serializer<object>.Utf8StringWriter(new StringBuilder());

            Assert.AreEqual(Encoding.UTF8, ioStream.Encoding);

        }
    }
}
