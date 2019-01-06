using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using CLMS.Framework.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if NETFRAMEWORK
using Http.TestLibrary;
#endif

namespace CLMS.Framework.Tests.Utilities
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
                Assert.AreEqual("127.0.0.1", Web.GetClientIp());
            }
#endif
        }
        
        [TestMethod]
        public void IsUserInRoleTest()
        {
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory()).SimulateRequest())
            {
//                Assert.AreEqual(false, Web.IsUserInRole("Admin"));
            }
#endif
        }
        
        [TestMethod]
        public void IsInControllerActionTest()
        {
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory())
                .SimulateRequest())
            {
                Assert.AreEqual(false, Web.IsInControllerAction("Test"));
            }
            
            using (new HttpSimulator("/", Directory.GetCurrentDirectory())
                .SimulateRequest(new Uri("http://clms.test.com?_currentControllerAction=Test")))
            {
                Assert.AreEqual(true, Web.IsInControllerAction("Test"));
            }
#endif
        }
        
        [TestMethod]
        public void GetQueryTest()
        {
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory())
                .SimulateRequest(new Uri("http://clms.test.com?_currentControllerAction=Test")))
            {
                Assert.AreEqual("?_currentControllerAction=Test", Web.GetQuery());
            }            
#endif
        }
        
        [TestMethod]
        public void GetFormArgument()
        {
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory())
                .SimulateRequest(new Uri("http://clms.test.com?returnUrl=www.google.com")))
            {
                Assert.AreEqual("www.google.com", Web.GetFormArgument("returnUrl"));
            }            
            
            using (new HttpSimulator("/", Directory.GetCurrentDirectory())
                .SimulateRequest(new Uri("http://clms.test.com")))
            {
                Assert.AreEqual("", Web.GetFormArgument("returnUrl"));
            }            
#endif
        }
        
        [TestMethod]
        public void GetRequestHeader()
        {
            var options = new NameValueCollection();

            var headers = new NameValueCollection {{"Accept", "text/plain"}};


#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory())
                .SimulateRequest(new Uri("http://clms.test.com"), options, headers))
            {
                Assert.AreEqual("text/plain", Web.GetRequestHeader("Accept"));
            }
#endif
        }
        
        [TestMethod]
        public void GetReturnUrl()
        {
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory())
                .SimulateRequest(new Uri("http://clms.test.com?returnUrl=www.google.com")))
            {
                Assert.AreEqual("www.google.com", Web.GetReturnUrl());
            }            
#endif
        }
        
        [TestMethod]
        public void SerializationSanitizationEntriesTest()
        {
            Assert.AreEqual(">>MVC_1<<", Web.SerializationSanitizationEntries["="]);
        }
        
        [TestMethod]
        public void CurrentServerRoleTest()
        {
            Assert.AreEqual(Web.ServerRole.Combined, Web.CurrentServerRole);
        }
        
        [TestMethod]
        public void MapPathTest()
        {
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory())
                .SimulateRequest(new Uri("http://clms.test.com?returnUrl=www.google.com")))
            {
                Assert.AreEqual(@"C:\Users\George Theofilis\Documents\Projects\Core\CLMS.Framework\test\CLMS.Framework.Tests\bin\Debug\net472\App_Data", Web.MapPath("App_Data"));
            } 
#endif
        }

        [TestMethod]
        public void SessionTest()
        {
#if NETFRAMEWORK
            using (new HttpSimulator("/", Directory.GetCurrentDirectory())
                .SimulateRequest(new Uri("http://clms.test.com?returnUrl=www.google.com")))
            {
                Debug.WriteLine(Web.Session.GetStorage());

                Assert.ThrowsException<AppDev.Cache.CacheException>(() => Web.Session.Get("Name"));
                
                Web.Session.Set("Name", "George");
                
                Assert.AreEqual("George", Web.Session.Get("Name"));
                
                Web.Session.Add("Phone", "808080");
                
                Assert.AreEqual("808080", Web.Session.Get("Phone"));
                
                Web.Session.Remove("Phone");
                
                Assert.ThrowsException<AppDev.Cache.CacheException>(() => Web.Session.Get("Phone"));
            } 
#endif      
        }
    }
}