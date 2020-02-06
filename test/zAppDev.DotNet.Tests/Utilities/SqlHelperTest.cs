// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using zAppDev.DotNet.Framework.Configuration;
using zAppDev.DotNet.Framework.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using zAppDev.DotNet.Framework.Data.DatabaseManagers.DatabaseUtilities;
using zAppDev.DotNet.Framework.Data.DatabaseManagers;

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

            services.AddSingleton(ins => ConfigurationHandler.GetAppConfiguration());


            var databaseManager = new MSSQLManager(ConfigurationHandler.GetConfiguration());
            services.AddSingleton(provider => databaseManager);

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
            Assert.IsNotNull(ServiceLocator.Current.GetInstance<MSSQLManager>().ConnectionString);
        }

        [TestMethod]
        public void GetCommandTimeoutTest()
        {
            var users = ServiceLocator.Current.GetInstance<MSSQLManager>().RunSqlQuery("SELECT * FROM [dbo].[Customer]");

            Assert.IsNotNull(users);
            Assert.AreEqual(91, users.Count);

            var parameters = new Dictionary<string, object>
            {
                {"Name", "Art"}
            };

            users = ServiceLocator.Current.GetInstance<MSSQLManager>().RunSqlQuery("SELECT * FROM [dbo].[Customer] WHERE [FirstName] = @Name", parameters, ServiceLocator.Current.GetInstance<MSSQLManager>().ConnectionString);

            Assert.IsNotNull(users);
            Assert.AreEqual(1, users.Count);

            users = ServiceLocator.Current.GetInstance<MSSQLManager>().RunSqlQuery("SELECT * FROM [dbo].[Customer] WHERE [FirstName] = @Name", parameters, 100, ServiceLocator.Current.GetInstance<MSSQLManager>().ConnectionString);

            Assert.IsNotNull(users);
            Assert.AreEqual(1, users.Count);
        }

        [TestMethod]
        public void RunStoredProcedureTest()
        {
            var result = ServiceLocator.Current.GetInstance<MSSQLManager>().RunStoredProcedure("Test");
            Assert.AreEqual(91, result?.Count);

            var parameters = new Dictionary<string, object>
            {
                {"Name", "Art"}
            };

            result = ServiceLocator.Current.GetInstance<MSSQLManager>().RunStoredProcedure("Test", ServiceLocator.Current.GetInstance<MSSQLManager>().ConnectionString);
            Assert.AreEqual(91, result?.Count);

            result = ServiceLocator.Current.GetInstance<MSSQLManager>().RunStoredProcedure("TestParam", parameters, ServiceLocator.Current.GetInstance<MSSQLManager>().ConnectionString);
            Assert.AreEqual(1, result?.Count);
        }
    }
#endif
}
