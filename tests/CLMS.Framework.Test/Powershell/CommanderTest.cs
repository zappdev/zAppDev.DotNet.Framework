using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Runspaces;
using CLMS.Framework.Powershell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Test.Powershell
{
    [TestClass]
    public class CommanderTest
    {
        private int _port = 5985;

        private int _sslPort = 5986;

        private string _shellUri = "http://schemas.microsoft.com/powershell/Microsoft.PowerShell";

        private string _appName = "/wsman";


        [TestMethod]
        public void CheckConstantTest()
        {
            Assert.AreEqual(_port, Commander.Port);
            Assert.AreEqual(_shellUri, Commander.ShellURI);
            Assert.AreEqual(_sslPort, Commander.SSLPort);
            Assert.AreEqual(_appName, Commander.AppName);
        }

        [TestMethod]
        public void RunPowerShellScriptTest()
        {
            Assert.ThrowsException<FileNotFoundException>((() => Commander.RunPowerShellScript("NotExists.ps1")));
            Assert.ThrowsException<EndOfStreamException>((() => Commander.RunPowerShellScript("EmptyScript.ps1")));

            var result = Commander.RunPowerShellScript("TestScript.ps1");
            
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("False", result[0]);
            Assert.AreEqual("", result[1]);
            
            
            result = Commander.RunPowerShellScript("TestScript.ps1", new Dictionary<string, object> { {"in", "test"} });
            
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("False", result[0]);
            Assert.AreEqual("test", result[1]);
            
            Assert.ThrowsException<Exception>((() => 
                Commander.RunPowerShellScript("TestScript.ps1", new Dictionary<string, object> { {"in", "raiseError"} })));

            Assert.ThrowsException<Exception>(() =>
                Commander.RunPowerShellScript("TestScript.ps1", new WSManConnectionInfo
                {                    
                    OperationTimeout = 1,
                    OpenTimeout = 1
                }));

        }

    }
}
