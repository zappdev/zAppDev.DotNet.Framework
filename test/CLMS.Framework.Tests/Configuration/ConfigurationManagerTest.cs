using System.Configuration;

using CLMS.Framework.Configuration;
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
            var config = ConfigurationHandler.GetAppConfiguration();

            Assert.IsNotNull(config.ConnectionStrings["Database"].ConnectionString);
            Assert.AreEqual("false", config.AppSettings["ZipWebRequests"]);
        }
    }
}
