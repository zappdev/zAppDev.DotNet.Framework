
using System;
using System.Configuration;

using CLMS.Framework.Configuration;
#if NETFRAMEWORK
#else
using Microsoft.Extensions.Configuration;
#endif

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Configuration
{
    [TestClass]
    public class ConfigurationManagerTest
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
        public void SetUpConfigurationBuilderTest()
        {
            const string expected = "Data Source=192.168.2.201;Initial Catalog=cfTests_4_gtheofilis;Integrated Security=False;User ID=g.theofilis@clmsuk.com;Password=f8743baeb8bc41939c3f6989d256f640c9953e790de84c839b0eb872e1a817ce1@";
#if NETFRAMEWORK
            Assert.AreEqual(expected, System.Configuration.ConfigurationManager.ConnectionStrings["Database"].ConnectionString);
#else
            var config = ConfigurationHandler
                .SetUpConfigurationBuilder(new ConfigurationBuilder())
                .Build();

            var appConfig = config.Get<AppConfig>();
            Assert.AreEqual(expected, appConfig.ConnectionStrings["Database"].ConnectionString);
#endif
        }
    }
}
