// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Utilities;
using CSharpVerbalExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Utilities
{
    [TestClass]
    public class RegExHelperTest
    {
        [TestMethod]
        public void GetMatchTest()
        {
            var verbEx = VerbalExpressions
                .DefaultExpression
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
            var verbEx = VerbalExpressions
                .DefaultExpression
                .StartOfLine()
                .Then("http")
                .Maybe("s")
                .Then("://")
                .Maybe("www.")
                .AnythingBut(" ")
                .EndOfLine();

            const string testMe = "https://www.google.com";

            var matches = RegExHelper.GetMatches(verbEx, testMe);

            Assert.AreEqual(1, matches.Count);

            matches = RegExHelper.GetMatches(verbEx, testMe + " ");

            Assert.AreEqual(null, matches);
        }
    }
}