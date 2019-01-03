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
            const string expected = "Data Source=192.168.2.201;Initial Catalog=cfTests_4_gtheofilis;Integrated Security=False;User ID=g.theofilis@clmsuk.com;Password=f8743baeb8bc41939c3f6989d256f640c9953e790de84c839b0eb872e1a817ce1@";
            Assert.AreEqual(expected, SqlHelper.GetConnectionString());
        }

        [TestMethod]
        public void GetCommandTimeoutTest()
        {
            var users = SqlHelper.RunSqlQuery("SELECT * FROM [security].[ApplicationUsers]");

            Assert.IsNotNull(users);

            var parameters = new Dictionary<string, object>
            {
                {"Name", ""}
            };

            users = SqlHelper.RunSqlQuery("SELECT * FROM [security].[ApplicationUsers] WHERE [UserName] = @Name", parameters, SqlHelper.GetConnectionString());

            Assert.IsNotNull(users);
        }

        [TestMethod]
        public void RunStoredProcedureTest()
        {
            var result = SqlHelper.RunStoredProcedure("Test");
            Assert.IsNotNull(result);
        }
    }
}
