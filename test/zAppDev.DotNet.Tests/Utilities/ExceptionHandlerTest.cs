using zAppDev.DotNet.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;

#if NETFRAMEWORK
using Http.TestLibrary;
#else
using Moq;
using Microsoft.AspNetCore.Http;
#endif

namespace zAppDev.DotNet.Framework.Tests.Utilities
{
    [TestClass]
    public class ExceptionHandlerTest
    {
        [TestMethod]
        public void ExceptionHelperTest()
        {
            var obj = new ExceptionHelper("test.js", 100);

            Assert.AreEqual(100, obj.lineNo);
            Assert.AreEqual("test.js", obj.filePath);
        }

        [TestMethod]
        public void MapTest()
        {
            var obj = new Map("app", "CondionalFormating", "id", 1);

            Assert.AreEqual("id", obj.AppDevIdentifier);
            Assert.AreEqual(AppDevSemantic.CondionalFormating, obj.AppDevSemantic);

            Assert.ThrowsException<ApplicationException>(() => new Map("app", "DontExist", "id", 1));
        }

        [TestMethod]
        public void LocationTest()
        {
            var obj = new Location();

            Assert.AreEqual(null, obj.End);
            Assert.AreEqual(null, obj.Start);
        }

        [TestMethod]
        public void FilePositionTest()
        {
            var obj = new FilePosition();

            Assert.AreEqual(0, obj.Column);
            Assert.AreEqual(0, obj.Line);
        }

        [TestMethod]
        public void FriendlyMessageEntryDtoTest()
        {
            var obj = new FriendlyMessageEntryDTO("id", "test");

            Assert.AreEqual("id", obj.AppdevIdentifier);
            Assert.AreEqual("test", obj.AppdevSemantic);
        }

        [TestMethod]
        public void ExceptionHandlerInitTest()
        {
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory()).SimulateRequest())
            {
                var obj = new ExceptionHandler();

            }
#else
            Helper.HttpCoreSimulate(() =>
            {
                var context = new DefaultHttpContext();

                context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
                return context;
            });

            var obj = new ExceptionHandler();
#endif
        }
    }
}
