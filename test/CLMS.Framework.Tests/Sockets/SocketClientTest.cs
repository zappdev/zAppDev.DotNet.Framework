using System;
using System.Diagnostics;
using System.Net.Sockets;
using CLMS.Framework.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CLMS.Framework.Tests.Sockets
{
    [TestClass]
    public class SocketClientTest
    {
        [TestMethod]
        public void CreateConnectionTest()
        {
            const string serviceName = "GoogleDNS";
            var client = SocketClient.CreateConnection(serviceName, "8.8.8.8", 53);
            
            Assert.ThrowsException<Exception>(() => SocketClient.CreateConnection(serviceName, "8.8.8.8", 53));
            
            Assert.IsTrue(client.IsConnected);
            Assert.IsInstanceOfType(client.TheSocket, typeof(Socket));
                       
            Assert.IsInstanceOfType(SocketClient.GetConnection(serviceName), typeof(SocketClient));
            Assert.ThrowsException<Exception>(() => SocketClient.GetConnection("NotExist"));


            client.Send("");
            
            client.StartReceiving(bytes =>
            {
                Debug.WriteLine(bytes);
                return true;
            });
            
            // SocketClient.CloseConnection(serviceName);
            // Assert.ThrowsException<Exception>(() => SocketClient.CloseConnection("NotExist"));
            
            client.Dispose();
        }

        [TestMethod]
        public void BadConnectionTest()
        {
#if Linux
            try
            {
                SocketClient.CreateConnection("GoogleDNS Bad", "8.8.8.8", 80);
            } 
            catch (Exception ex)
            {
                
            }
#else
            Assert.ThrowsException<SocketException>(() => SocketClient.CreateConnection("GoogleDNS Bad", "8.8.8.8", 80));
#endif
        }
    }
}
