using zAppDev.DotNet.Framework.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if NETFRAMEWORK
#else
using System.Configuration;
#endif

namespace zAppDev.DotNet.Framework.Tests.Configuration
{
#if NETFRAMEWORK
#else
    [TestClass]
    public class ConfigurationManagerTest
    {
        [TestInitialize]
        public void Initialize()
        {
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
        }

        [TestMethod]
        public void SetUpConfigurationBuilderTest()
        {
            var config = ConfigurationHandler.GetAppConfiguration();

            Assert.IsNotNull(config.ConnectionStrings["Database"].ConnectionString);
            Assert.AreEqual("false", config.AppSettings["ZipWebRequests"]);
        }

        [TestMethod]
        public void GetAppConfigurationTest()
        {
            var config = ConfigurationHandler
                .SetUpConfigurationBuilder(new ConfigurationBuilder())
                .Build();

            var appConfig = config.Get<AppConfiguration>();

            Assert.IsNotNull(appConfig.ConnectionStrings["Database"].ConnectionString);
            Assert.AreEqual("false", appConfig.AppSettings["ZipWebRequests"]);
        }
    }
#endif
}