using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Data
{
    [TestClass]
    public class LinqRuntimeTypeBuilderTest
    {
        [TestMethod()]
        public void PredicateTest()
        {
            var id = LinqRuntimeTypeBuilder.LinqRuntimeTypeBuilder.SanitizeCSharpIdentifier("true");

        }
    }
}
