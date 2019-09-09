// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using zAppDev.DotNet.Framework.Powershell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace zAppDev.DotNet.Framework.Tests.Powershell
{
    internal class Result
    {
        public int Test { get; set; }
    }
    
    [TestClass]
    public class AdapterTest
    {
        [TestMethod]
        public void GetPowershellResults()
        {
            var adapter = new Adapter<Result>();

            var result = adapter.GetPowershellResults("./Assets/JSON.ps1", new Dictionary<string, object> { {"in", "more" }});
            
            Assert.AreEqual(3, result.Result.Count);
            Assert.AreEqual(true, result.Successful);
            
            adapter.GetPowershellResult("./Assets/JSON.ps1");
            
            var adapterIssue = new Adapter<string>();

            Assert.ThrowsException<JsonReaderException>(() => { adapterIssue.GetPowershellResult("./Assets/JSON.ps1", new Dictionary<string, object> { {"in", "ok" }}); });
        }

        [TestMethod]
        public void GetPowershellResult()
        {
            var adapter = new Adapter<Result>();

            var result = adapter.GetPowershellResult("./Assets/JSON.ps1", new Dictionary<string, object> { {"in", "ok" }});
            
            Assert.AreEqual(0, result.Result.Test);
            Assert.AreEqual(true, result.Successful);
            
            adapter.GetPowershellResult("./Assets/JSON.ps1");
            
            var adapterIssue = new Adapter<string>();

            Assert.ThrowsException<JsonReaderException>(() => { adapterIssue.GetPowershellResult("./Assets/JSON.ps1", new Dictionary<string, object> { {"in", "ok" }}); });
        } 
    }
}
