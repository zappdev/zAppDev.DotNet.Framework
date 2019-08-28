using System;
using System.IO;
using System.Net;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.RegularExpressions;
using zAppDev.DotNet.Framework.Utilities;

using Microsoft.VisualStudio.TestTools.UnitTesting;

#if NETFRAMEWORK
using Http.TestLibrary;
#else
using Moq;
using Microsoft.AspNetCore.Http;
#endif

namespace zAppDev.DotNet.Framework.Tests.Utilities
{
    [TestClass]
    public class WebTest
    {
        [TestMethod]
        public void GetClientIpTest()
        {
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory()).SimulateRequest())
            {
                Assert.AreEqual("127.0.0.1", Framework.Utilities.Web.GetClientIp());
            }
#else
            Helper.HttpCoreSimulate(() =>
            {
                var context = new DefaultHttpContext();

                context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
                return context;
            });

            Assert.AreEqual("127.0.0.1", Framework.Utilities.Web.GetClientIp());
#endif
        }

        [TestMethod]
        public void GetRequestUrl()
        {
            var url = "http://clms.test.com/Add?_currentControllerAction=Test";
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory()).SimulateRequest(new Uri(url)))
            {
                Assert.AreEqual(url, Framework.Utilities.Web.GetRequestUri().ToString());
            }
#else
            Helper.HttpCoreSimulate(() =>
            {
                var context = new DefaultHttpContext();

                context.Request.Scheme = "http";
                context.Request.Host = new HostString("clms.test.com");
                context.Request.Path = new PathString("/Add");
                context.Request.QueryString = new QueryString("?_currentControllerAction=Test");
                return context;
            });

            Assert.AreEqual(url, Framework.Utilities.Web.GetRequestUri().ToString());
#endif
        }

        [TestMethod]
        public void IsUserInRoleTest()
        {
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory()).SimulateRequest())
            {
//                Assert.AreEqual(false, Framework.Utilities.Web.IsUserInRole("Admin"));
            }
#else
            Helper.HttpCoreSimulate(() =>
            {
                var context = new DefaultHttpContext();

                context.User.AddIdentity(new ClaimsIdentity());
                return context;
            });
            Assert.AreEqual(false, Framework.Utilities.Web.IsUserInRole("Admin"));
#endif
        }

        [TestMethod]
        public void IsUserTest()
        {
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory()).SimulateRequest())
            {
//                Assert.AreEqual(false, Framework.Utilities.Web.IsUserInRole("Admin"));
            }
#else
            Helper.HttpCoreSimulate(() =>
            {
                var context = new DefaultHttpContext();

                context.User.AddIdentity(new ClaimsIdentity());
                return context;
            });
            Assert.AreEqual(false, Framework.Utilities.Web.IsUser("Admin"));
#endif
        }

        [TestMethod]
        public void IsInControllerActionTest()
        {
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory())
                .SimulateRequest())
            {
                Assert.AreEqual(false, Framework.Utilities.Web.IsInControllerAction("Test"));
            }
            
            using (new HttpSimulator("/", Directory.GetCurrentDirectory())
                .SimulateRequest(new Uri("http://clms.test.com?_currentControllerAction=Test")))
            {
                Assert.AreEqual(true, Framework.Utilities.Web.IsInControllerAction("Test"));
            }
#else
            Helper.HttpCoreSimulate(() =>
            {
                var context = new DefaultHttpContext();

                context.Request.QueryString = new QueryString("?_currentControllerAction=Test");

                return context;
            });

            Assert.AreEqual(true, Framework.Utilities.Web.IsInControllerAction("Test"));
#endif
        }

        [TestMethod]
        public void GetQueryTest()
        {
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory())
                .SimulateRequest(new Uri("http://clms.test.com?_currentControllerAction=Test")))
            {
                Assert.AreEqual("?_currentControllerAction=Test", Framework.Utilities.Web.GetQuery());
            }
#else
            Helper.HttpCoreSimulate(() =>
            {
                var context = new DefaultHttpContext();

                context.Request.QueryString = new QueryString("?_currentControllerAction=Test");

                return context;
            });

            Assert.AreEqual("?_currentControllerAction=Test", Framework.Utilities.Web.GetQuery());
#endif
        }

        [TestMethod]
        public void GetFormArgument()
        {
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory())
                .SimulateRequest(new Uri("http://clms.test.com?returnUrl=www.google.com")))
            {
                Assert.AreEqual("www.google.com", Framework.Utilities.Web.GetFormArgument("returnUrl"));
            }            
            
            using (new HttpSimulator("/", Directory.GetCurrentDirectory())
                .SimulateRequest(new Uri("http://clms.test.com")))
            {
                Assert.AreEqual("", Framework.Utilities.Web.GetFormArgument("returnUrl"));
            }            
#else
            Helper.HttpCoreSimulate(() =>
            {
                var context = new DefaultHttpContext();

                context.Request.QueryString = new QueryString("?_currentControllerAction=Test");

                return context;
            });
            Assert.AreEqual("Test", Framework.Utilities.Web.GetFormArgument("_currentControllerAction"));
            Assert.AreEqual("", Framework.Utilities.Web.GetFormArgument("returnUrl"));
#endif
        }

        [TestMethod]
        public void GetRequestHeader()
        {
            var options = new NameValueCollection();

            var headers = new NameValueCollection { { "Accept", "text/plain" } };


#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory())
                .SimulateRequest(new Uri("http://clms.test.com"), options, headers))
            {
                Assert.AreEqual("text/plain", Framework.Utilities.Web.GetRequestHeader("Accept"));
            }
#else
            Helper.HttpCoreSimulate(() =>
            {
                var context = new DefaultHttpContext();

                context.Request.Headers.Add("Accept", "text/plain");

                return context;
            });
            Assert.AreEqual("text/plain", Framework.Utilities.Web.GetRequestHeader("Accept"));
#endif
        }

        [TestMethod]
        public void GetReturnUrl()
        {
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory())
                .SimulateRequest(new Uri("http://clms.test.com?returnUrl=www.google.com")))
            {
                Assert.AreEqual("www.google.com", Framework.Utilities.Web.GetReturnUrl());
            }
#else
            Helper.HttpCoreSimulate(() =>
            {
                var context = new DefaultHttpContext();

                context.Request.QueryString = new QueryString("?returnUrl=www.google.com");

                return context;
            });

            Assert.AreEqual("www.google.com", Framework.Utilities.Web.GetReturnUrl());

            Helper.HttpCoreSimulate(() =>
            {
                var context = new DefaultHttpContext();

                context.Request.Headers.Add("Referer", "returnUrl=www.google.com");

                return context;
            });

            Assert.AreEqual("www.google.com", Framework.Utilities.Web.GetReturnUrl());
#endif
        }

        [TestMethod]
        public void SerializationSanitizationEntriesTest()
        {
            Assert.AreEqual(">>MVC_1<<", Framework.Utilities.Web.SerializationSanitizationEntries["="]);
        }

        [TestMethod]
        public void CurrentServerRoleTest()
        {
            Assert.AreEqual(Framework.Utilities.Web.ServerRole.Combined, Framework.Utilities.Web.CurrentServerRole);
        }

        [TestMethod]
        public void MapPathTest()
        {
            var regex = new Regex(@"^(.+)(\\|\/)([^(\\|\/)]+)$");
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory())
                .SimulateRequest(new Uri("http://clms.test.com?returnUrl=www.google.com")))
            {                
                Assert.IsTrue(regex.IsMatch(Framework.Utilities.Web.MapPath("App_Data")));
                Assert.IsTrue(regex.IsMatch(Framework.Utilities.Web.MapPath("~/App_Data/CodeMap.js")));
            }
#else
            Helper.HttpCoreSimulate(() =>
            {
                var context = new DefaultHttpContext();

                context.Request.Headers.Add("Referer", "returnUrl=www.google.com");

                return context;
            });
            Assert.IsTrue(regex.IsMatch(Framework.Utilities.Web.MapPath("App_Data")));
#endif
        }

        [TestMethod]
        public void SessionTest()
        {
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory())
                .SimulateRequest(new Uri("http://clms.test.com?returnUrl=www.google.com")))
            {
                Debug.WriteLine(Framework.Utilities.Web.Session.GetStorage());

                Assert.ThrowsException<CLMS.AppDev.Cache.CacheException>(() => Framework.Utilities.Web.Session.Get("Name"));
                
                Framework.Utilities.Web.Session.Set("Name", "George");
                
                Assert.AreEqual("George", Framework.Utilities.Web.Session.Get("Name"));
                
                Framework.Utilities.Web.Session.Add("Phone", "808080");
                
                Assert.AreEqual("808080", Framework.Utilities.Web.Session.Get("Phone"));
                
                Framework.Utilities.Web.Session.Remove("Phone");
                
                Assert.ThrowsException<CLMS.AppDev.Cache.CacheException>(() => Framework.Utilities.Web.Session.Get("Phone"));
            } 
#else
            Helper.HttpCoreSimulate(() =>
            {
                var context = new DefaultHttpContext();

                context.Request.Headers.Add("Referer", "returnUrl=www.google.com");

                var mockSession = new Mock<ISession>();
                mockSession.Setup(_ => _.Id).Returns("dqe3wrd");
                context.Session = mockSession.Object;

                return context;
            });

            //Debug.WriteLine(Framework.Utilities.Web.Session.GetStorage());

            //Assert.ThrowsException<AppDev.Cache.CacheException>(() => Framework.Utilities.Web.Session.Get("Name"));

            //Framework.Utilities.Web.Session.Set("Name", "George");

            //Assert.AreEqual("George", Framework.Utilities.Web.Session.Get("Name"));

            //Framework.Utilities.Web.Session.Add("Phone", "808080");

            //Assert.AreEqual("808080", Framework.Utilities.Web.Session.Get("Phone"));

            //Framework.Utilities.Web.Session.Remove("Phone");

            //Assert.ThrowsException<AppDev.Cache.CacheException>(() => Framework.Utilities.Web.Session.Get("Phone"));
#endif
        }
    }
}