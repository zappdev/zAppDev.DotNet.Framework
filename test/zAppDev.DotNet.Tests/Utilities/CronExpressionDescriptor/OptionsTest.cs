// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using zAppDev.DotNet.Framework.Utilities.CronExpressionDescriptor;

namespace zAppDev.DotNet.Framework.Tests.Utilities.CronExpressionDescriptor
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
