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
            Assert.IsNotNull(operation.Permissions);
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

        [TestMethod()]
        public void ApplicationRoleTest()
        {
            var operation = new ApplicationRole()
            {
                Id = 0,
                Name = "",
                IsCustom = true,
                Permissions = null
            };

            Assert.AreEqual("", operation.Name);
            Assert.AreEqual(0, operation.Id);
            Assert.AreEqual(null, operation.Permissions);
            Assert.AreEqual(true, operation.IsCustom);
        }

        [TestMethod()]
        public void ApplicationUserTest()
        {
            var operation = new ApplicationUser()
            {
                EmailConfirmed = true,
                LockoutEnabled = true,
                Permissions =  null,
                Profile = null,
                Roles = null,
                Name = "",
                Email = "",
                PhoneNumber = "",
                UserName = ""
            };

            Assert.AreEqual(true, operation.LockoutEnabled);
            Assert.AreEqual(true, operation.EmailConfirmed);
            Assert.AreEqual("", operation.Name);
            Assert.AreEqual("", operation.Email);
            Assert.AreEqual("", operation.PhoneNumber);
            Assert.AreEqual("", operation.UserName);
            Assert.AreEqual(null, operation.Permissions);
            Assert.AreEqual(null, operation.Profile);
            Assert.AreEqual(null, operation.Roles);
        }

        [TestMethod()]
        public void DateTimeFormatTest()
        {
            var operation = new DateTimeFormat
            {
                ApplicationLanguage = null,
                Id = 0,
                LongDatePattern = "",
                LongTimePattern =  "",
                MonthDayPattern =  "",
                RFC1123Pattern = "",
                ShortDatePattern = "",
                ShortTimePattern = "",
                YearMonthPattern = ""
            };

            Assert.AreEqual(null, operation.ApplicationLanguage);
            Assert.AreEqual(0, operation.Id);
            Assert.AreEqual("", operation.LongDatePattern);
            Assert.AreEqual("", operation.LongTimePattern);
            Assert.AreEqual("", operation.MonthDayPattern);
            Assert.AreEqual("", operation.RFC1123Pattern);
            Assert.AreEqual("", operation.ShortDatePattern);
            Assert.AreEqual("", operation.ShortTimePattern);
            Assert.AreEqual("", operation.YearMonthPattern);
        }
    }
}
