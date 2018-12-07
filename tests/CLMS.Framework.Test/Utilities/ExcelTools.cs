using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using static CLMS.Framework.Utilities.ExcelTools;

namespace CLMS.Framework.Test.Utilities
{
    [TestClass]
    public class ExcelTools
    {
        [TestMethod]
        public void ExportExcelOptionsTest()
        {
            var option = new ExportExcelOptions
            {
                Columns = new List<object>(),
                Values = new List<List<object>> {
                    new List<object> { "0", "1" },
                    new List<object> { "2", "3" }
                },
                ColumnFormattings = { "Col1", "Col2" },
                Title = "Info.xls",
                Author = "George Theofilis"
            };

            Assert.AreEqual("Info.xls", option.Title);
            Assert.AreEqual("George Theofilis", option.Author);
            Assert.AreEqual(null, option.Path);

            Assert.AreEqual(false, option.IsValid);

            option.Path = "";

            Assert.AreEqual(false, option.IsValid);
        }
    }
}
