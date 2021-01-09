// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using zAppDev.DotNet.Framework.Data;
using zAppDev.DotNet.Framework.Data.DAL;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using zAppDev.DotNet.Framework.Mvc;
using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Tests.Data
{
    [TestClass]
    public class ExportHelperV2Test
    {     
        [TestInitialize]
        public void Initialize()
        {
        }

        [TestMethod]
        public void TestCSV()
        {
            ExportOptionsV2 exportOptions = new ExportOptionsV2
            {
                Type = ExportHelper.Type.CSV,
                Filename = "nemo",
                CsvSeperator = ",",
                CsvDontAddHeder = false
            };

            var exportHelper = new ExportHelperV2<TempClassToExport>(exportOptions,
                new Dictionary<string, Func<TempClassToExport, object>>
            {
                    {"Name" , item => item?.Name },
                    {"Count" , item => item?.Count },
            });

            var items = new List<TempClassToExport>
            {
                new TempClassToExport(1, "nemo"),
                new TempClassToExport(2, "hemo"),
            };

            if (exportHelper.Options.ColumnInfo == null)
            {
                exportHelper.Options.ColumnInfo = new List<ColumnOptionsV2>
                {
                    new ColumnOptionsV2 { Caption = "Name", Formatting = "", Name = "Name" },
                    new ColumnOptionsV2 { Caption = "Count", Formatting = "", Name = "Count" },

                };
            }

            var path = exportHelper.ExportToCSV(items);

            Assert.IsTrue(System.IO.File.Exists(path));
        }
    }

    class TempClassToExport
    {
        public TempClassToExport(int count, string name)
        {
            Count = count;
            Name = name;
        }

        public int Count { get; set; }
        public string Name { get; set; }
    }
}
