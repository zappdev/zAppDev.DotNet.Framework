// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using zAppDev.DotNet.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Utilities
{
    [TestClass]
    public class DebugHelperTest
    {
        [TestMethod]
        public void LogTest()
        {
            DebugHelper.Log(DebugMessageType.Debug, null, "Test");
            DebugHelper.Log(DebugMessageType.Debug, "LogTest", "Test");
            DebugHelper.Log(DebugMessageType.Error, "LogTest", "Test");
            DebugHelper.Log(DebugMessageType.Info, "LogTest", "Test");
            DebugHelper.Log(DebugMessageType.Warning, "LogTest", "Test");
            DebugHelper.Log(DebugMessageType.IDEF0Trace, "LogTest", "Test");

            Assert.ThrowsException<ArgumentOutOfRangeException>((() =>
                    DebugHelper.Log((DebugMessageType) 10, "LogTest", "Test"))
            );

            void RaiseDelegated(string type, string message)
            {
            }

            DebugHelper.Log(DebugMessageType.Debug, "LogTest", RaiseDelegated, "Test");
            DebugHelper.Log(DebugMessageType.Debug, "LogTest", RaiseDelegated, "Test", true);
            DebugHelper.Log(DebugMessageType.Debug, null, RaiseDelegated, "Test");

            DebugHelper.Log(DebugMessageType.Debug, RaiseDelegated, "Test");

            DebugHelper.Log(new List<string> {"1"}, "LogTest");
        }
    }
}