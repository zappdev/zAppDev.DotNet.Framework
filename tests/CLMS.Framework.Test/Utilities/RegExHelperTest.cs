using CLMS.Framework.Utilities;
using CSharpVerbalExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Test.Utilities
{
    [TestClass]
    public class RegExHelperTest
    {
        [TestMethod]
        public void GetMatchTest()
        {
            var verbEx = new VerbalExpressions()
                        .StartOfLine()
                        .Then("http")
                        .Maybe("s")
                        .Then("://")
                        .Maybe("www.")
                        .AnythingBut(" ")
                        .EndOfLine();

            var testMe = "https://www.google.com";

            var matches = RegExHelper.GetMatch(verbEx, testMe);

            Assert.AreEqual(testMe, matches);

            matches = RegExHelper.GetMatch(verbEx, testMe + " ");

            Assert.AreEqual(null, matches);
        }
        
        [TestMethod]
        public void GetMatchesTest()
        {
            var verbEx = new VerbalExpressions()
                        .StartOfLine()
                        .Then("http")
                        .Maybe("s")
                        .Then("://")
                        .Maybe("www.")
                        .AnythingBut(" ")
                        .EndOfLine();

            var testMe = "https://www.google.com";

            var matches = RegExHelper.GetMatches(verbEx, testMe);

            Assert.AreEqual(1, matches.Count);

            matches = RegExHelper.GetMatches(verbEx, testMe + " ");

            Assert.AreEqual(null, matches);
        }
    }
}
