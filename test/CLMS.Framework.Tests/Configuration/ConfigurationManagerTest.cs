
using System;
using System.Configuration;

using CLMS.Framework.Configuration;
using Microsoft.Extensions.Configuration;
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
#if NETFRAMEWORK
            Assert.IsNotNull(System.Configuration.ConfigurationManager.ConnectionStrings["Database"].ConnectionString);
#else
            var config = ConfigurationHandler
                .SetUpConfigurationBuilder(new ConfigurationBuilder())
                .Build();

            var appConfig = config.Get<AppConfig>();
            Assert.IsNotNull(appConfig.ConnectionStrings["Database"].ConnectionString);
#endif
        }
    }
}
