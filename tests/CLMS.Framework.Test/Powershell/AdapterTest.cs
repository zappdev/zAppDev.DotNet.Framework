using System;
using System.Collections.Generic;
using CLMS.Framework.Powershell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace CLMS.Framework.Tests.Powershell
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

            var result = adapter.GetPowershellResults("JSON.ps1", new Dictionary<string, object> { {"in", "more" }});
            
            Assert.AreEqual(3, result.Result.Count);
            Assert.AreEqual(true, result.Successful);
            
            adapter.GetPowershellResult("JSON.ps1");
            
            var adapterIssue = new Adapter<string>();

            Assert.ThrowsException<JsonReaderException>(() => { adapterIssue.GetPowershellResult("JSON.ps1", new Dictionary<string, object> { {"in", "ok" }}); });
        }

        [TestMethod]
        public void GetPowershellResult()
        {
            var adapter = new Adapter<Result>();

            var result = adapter.GetPowershellResult("JSON.ps1", new Dictionary<string, object> { {"in", "ok" }});
            
            Assert.AreEqual(0, result.Result.Test);
            Assert.AreEqual(true, result.Successful);
            
            adapter.GetPowershellResult("JSON.ps1");
            
            var adapterIssue = new Adapter<string>();

            Assert.ThrowsException<JsonReaderException>(() => { adapterIssue.GetPowershellResult("JSON.ps1", new Dictionary<string, object> { {"in", "ok" }}); });
        } 
    }
}
