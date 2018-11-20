using System.Collections.Generic;

using CLMS.Framework.SecurityDataDtos;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.SecurityDataDtos
{
    [TestClass]
    public class SecurityDataDtosTest
    {
        [TestMethod()]
        public void ApplicationOperationTest()
        {
            var operation = new ApplicationOperation {
                Id = 0,
                Name = "name",
                ParentControllerName = "parent",
                Type = "type",
                IsAvailableToAnonymous = true,
                IsAvailableToAllAuthorizedUsers = true,
                Permissions = new List<ApplicationPermission>()
            };

            Assert.AreEqual(0, operation.Id);
            Assert.AreEqual("name", operation.Name);
            Assert.AreEqual("parent", operation.ParentControllerName);
            Assert.AreEqual("type", operation.Type);
            Assert.AreEqual(true, operation.IsAvailableToAnonymous);
            Assert.AreEqual(true, operation.IsAvailableToAllAuthorizedUsers);
        }

        [TestMethod()]
        public void ApplicationLanguageTest()
        {
            var operation = new ApplicationLanguage {
                Id = 0,
                Icon  = null,
                Code  = "code",
                Name  = "name",
                DateTimeFormat  = null
            };

            Assert.AreEqual(0, operation.Id);
            Assert.AreEqual("name", operation.Name);
            Assert.AreEqual("code", operation.Code);
            Assert.AreEqual(null, operation.Icon);
            Assert.AreEqual(null, operation.DateTimeFormat);
        }

        [TestMethod()]
        public void ApplicationPermissionTest()
        {
            var operation = new ApplicationPermission {
                Id = 0,
                IsCustom   = true,
                Name  = "name"
            };

            Assert.AreEqual(0, operation.Id);
            Assert.AreEqual("name", operation.Name);
            Assert.AreEqual(true, operation.IsCustom);
        }

        [TestMethod()]
        public void ProfileTest()
        {
            var operation = new Profile {
                Id = 0,
                LanguageLCID = 0,
                LocaleLCID = 0,
                Theme = "theme"
            };

            Assert.AreEqual(0, operation.Id);
            Assert.AreEqual("theme", operation.Theme);
            Assert.AreEqual(0, operation.LanguageLCID);
            Assert.AreEqual(0, operation.LocaleLCID);
        }

        [TestMethod()]
        public void ApplicationThemeTest()
        {
            var operation = new ApplicationTheme {
                Name = "theme"
            };

            Assert.AreEqual("theme", operation.Name);
        }
    }
}
