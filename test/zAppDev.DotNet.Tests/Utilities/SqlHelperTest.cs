// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using zAppDev.DotNet.Framework.Configuration;
using zAppDev.DotNet.Framework.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace zAppDev.DotNet.Framework.Tests.Utilities
{
#if NETFRAMEWORK
#else
    [TestClass]
    public class SqlHelperTest
    {
        [TestInitialize]
        public void Initialize()
        {
            var services = new ServiceCollection();
            services.AddSingleton(ins => ConfigurationHandler.GetConfiguration());
            ServiceLocator.SetLocatorProvider(services.BuildServiceProvider());
        }

        [TestCleanup]
        public void Cleanup()
        {
            ServiceLocator.SetLocatorProvider(null);
        }

        [TestMethod]
        public void GetConnectionStringTest()
        {
            Assert.IsNotNull(SqlHelper.GetConnectionString());
        }

        [TestMethod]
        public void GetCommandTimeoutTest()
        {
            var users = SqlHelper.RunSqlQuery("SELECT * FROM [dbo].[Customer]");

            Assert.IsNotNull(users);
            Assert.AreEqual(91, users.Count);

            var parameters = new Dictionary<string, object>
            {
                {"Name", "Art"}
            };

            users = SqlHelper.RunSqlQuery("SELECT * FROM [dbo].[Customer] WHERE [FirstName] = @Name", parameters, SqlHelper.GetConnectionString());

            Assert.IsNotNull(users);
            Assert.AreEqual(1, users.Count);

            users = SqlHelper.RunSqlQuery("SELECT * FROM [dbo].[Customer] WHERE [FirstName] = @Name", parameters, 100, SqlHelper.GetConnectionString());

            Assert.IsNotNull(users);
            Assert.AreEqual(1, users.Count);
        }

        [TestMethod]
        public void RunStoredProcedureTest()
        {
            var result = SqlHelper.RunStoredProcedure("Test");
            Assert.AreEqual(91, result?.Count);

            var parameters = new Dictionary<string, object>
            {
                {"Name", "Art"}
            };

            result = SqlHelper.RunStoredProcedureWithConnectionString("Test", SqlHelper.GetConnectionString());
            Assert.AreEqual(91, result?.Count);

            result = SqlHelper.RunStoredProcedure("TestParam", parameters, SqlHelper.GetConnectionString());
            Assert.AreEqual(1, result?.Count);
        }
    }
#endif
}
