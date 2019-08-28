using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using static zAppDev.DotNet.Framework.Utilities.ExcelTools;

using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Tests.Utilities
{
    [TestClass]
    public class ExcelToolsTest
    {
        [TestMethod]
        public void ExportExcelOptionsTest()
        {
            var option = new ExportExcelOptions
            {
                Columns = { "Col1", "Col2" },
                Values = new List<List<object>> {
                    new List<object> { 0, 1 },
                    new List<object> { 2, 3 }
                },
                ColumnFormattings = { "Col1", "Col2" },
                Title = "Info",
                Author = "George Theofilis"
            };

            Assert.AreEqual("Info", option.Title);
            Assert.AreEqual("George Theofilis", option.Author);
            Assert.AreEqual(null, option.Path);

            Assert.AreEqual(false, option.IsValid);

            option.Path = "";

            Assert.AreEqual(false, option.IsValid);

            option.Path = "Book.xlsx";

            Assert.AreEqual(true, option.IsValid);

            ExportExcelFile(option);
        }
    }
}
