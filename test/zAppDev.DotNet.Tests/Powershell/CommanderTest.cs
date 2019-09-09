// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation.Runspaces;
using zAppDev.DotNet.Framework.Powershell;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Powershell
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
            Assert.ThrowsException<FileNotFoundException>((() => Commander.RunPowerShellScript("./Assets/NotExists.ps1")));
            Assert.ThrowsException<EndOfStreamException>((() => Commander.RunPowerShellScript("./Assets/EmptyScript.ps1")));

            var result = Commander.RunPowerShellScript("./Assets/TestScript.ps1");
            
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(false, result[0].BaseObject);

            Assert.AreEqual("", result[1].BaseObject);

            Assert.AreEqual(true, result.SingleObject());
            
            result = Commander.RunPowerShellScript("./Assets/TestScript.ps1", new Dictionary<string, object> { {"in", "test"} });
            
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(false, result[0].BaseObject);
            Assert.AreEqual("test", result[1].BaseObject);
            
            Assert.ThrowsException<Exception>((() => 
                Commander.RunPowerShellScript("./Assets/TestScript.ps1", new Dictionary<string, object> { {"in", "raiseError"} })));
#if OS_WINDOWS
            Assert.ThrowsException<Exception>(() =>
                Commander.RunPowerShellScript("TestScript.ps1", new WSManConnectionInfo(new Uri("http://www.contoso.com/"))
                {   
                    OperationTimeout = 1,
                    OpenTimeout = 1
                }));
#endif
        }

    }
}
