using System.Collections.Generic;
using System.Configuration;
using CLMS.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Utilities
{
    [TestClass]
    public class SqlHelperTest
    {
        [TestInitialize]
        public void Initialize()
        {
#if NETFRAMEWORK
#else
            var currentConfig =
                ConfigurationManager.OpenExeConfiguration(
                    ConfigurationUserLevel.None);

            var configFileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = "App.config"
            };

            var validConfig =
                ConfigurationManager.OpenMappedExeConfiguration(
                    configFileMap, ConfigurationUserLevel.None);

            validConfig.SaveAs(currentConfig.FilePath, ConfigurationSaveMode.Full);
#endif
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
}
