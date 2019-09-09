// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace zAppDev.DotNet.Framework.Sockets
{
    public class SocketClient : IDisposable
    {
        private const int ChunkSize = 2048;
        private readonly IPEndPoint _addr;
        private readonly System.Net.Sockets.Socket _socket;
        private readonly UTF8Encoding _encoder = new UTF8Encoding();
        private static readonly Dictionary<string, SocketClient> _sockets = new Dictionary<string, SocketClient>();
        private readonly BufferManager _bufferManager;
        //private ILog LogManager.GetLogger(this.GetType());
        private readonly MessageHandler _messageHandler;

        public Socket TheSocket => _socket;

        public bool IsConnected => _socket.Connected;

        public static SocketClient CreateConnection(string name, string ip, int port)
        {
            if (_sockets.ContainsKey(name)) throw new Exception($"A connection with name {name} already exist");

            var socket = new SocketClient(ip, port);
            _sockets.Add(name, socket);

            socket.Connect();

            return socket;
        }

        public static SocketClient GetConnection(string name)
        {
            if (!_sockets.ContainsKey(name)) throw new Exception($"No connection with name {name} exists");
            return _sockets[name];
        }

        public static void CloseConnection(string name)
        {
            if (!_sockets.ContainsKey(name)) throw new Exception($"No connection with name {name} exists");

            var sock = _sockets[name];

            var e = new SocketAsyncEventArgs();
            sock._bufferManager.SetBuffer(e);

            var bytes = Encoding.UTF8.GetBytes("");
            var recSendToken = new DataHoldingUserToken(bytes, Guid.NewGuid().ToString(), null);
            recSendToken.CreateNewDataHolder();
            e.UserToken = recSendToken;
            sock.StartDisconnect(e);
            _sockets.Remove(name);
        }

        private SocketClient(string ipAdd, int port)
        {
            //_logger = LogManager.GetLogger(this.GetType());
            _bufferManager = new BufferManager(ChunkSize, ChunkSize);
            _bufferManager.InitBuffer();
            _messageHandler = new MessageHandler();

            //_bufferManager 
            //    = new BufferManager(this.socketClientSettings.BufferSize * this.socketClientSettings.NumberOfSaeaForRecSend * this.socketClientSettings.OpsToPreAllocate, 
            //                        this.socketClientSettings.BufferSize * this.socketClientSettings.OpsToPreAllocate);

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);
            _addr = new IPEndPoint(IPAddress.Parse(ipAdd), port);
        }

        private void Connect()
        {
            _socket.Connect(_addr);
        }

        public void StartReceiving(Func<byte[], bool> onReceive, string messageDelimiter = "")
        {
            var bytes = Encoding.UTF8.GetBytes(messageDelimiter);
            StartReceiving(onReceive, bytes);
        }

        public void StartReceiving(Func<byte[], bool> onReceive, byte[] messageDelimiter)
        {
            var e = new SocketAsyncEventArgs();
            _bufferManager.SetBuffer(e);

            var recSendToken = new DataHoldingUserToken(messageDelimiter, Guid.NewGuid().ToString(), onReceive);

            //Create an object that we can write data to, and remove as an object
            //from the UserToken, if we wish.
            recSendToken.CreateNewDataHolder();

            e.UserToken = recSendToken;

            e.Completed += (sender, args) => {

                try
                {
                    LogManager.GetLogger(this.GetType()).Debug("IO_Completed method accessed with op: " + args.LastOperation);

                    // determine which type of operation just completed and call the associated handler
                    switch (args.LastOperation)
                    {
                        case SocketAsyncOperation.Connect:
                            LogManager.GetLogger(this.GetType()).Debug("IO_Completed method In Connect. NOT IMPLEMENTED!");
                            //ProcessConnect(e);
                            break;

                        case SocketAsyncOperation.Receive:
                            var recToken = (DataHoldingUserToken)e.UserToken;
                            LogManager.GetLogger(this.GetType()).Debug("IO_Completed method In Receive, id = " + recToken.TokenId);
                            ProcessReceive(args);

                            break;

                        case SocketAsyncOperation.Send:
                            var sendToken = (DataHoldingUserToken)e.UserToken;
                            LogManager.GetLogger(this.GetType()).Debug("IO_Completed method In Send, id = " + sendToken.TokenId + ". NOT IMPLEMENTED!");
                            //ProcessSend(e);
                            break;

                        case SocketAsyncOperation.Disconnect:
                            var disconnectToken = (DataHoldingUserToken)e.UserToken;
                            LogManager.GetLogger(this.GetType()).Debug("IO_Completed method In Disconnect, id = " + disconnectToken.TokenId + ". NOT IMPLEMENTED!");
                            //ProcessDisconnectAndCloseSocket(e);
                            break;

                        default:
                            {
                                var errorToken = (DataHoldingUserToken)e.UserToken;
                                LogManager.GetLogger(this.GetType()).Debug("Error in I/O Completed, id = " + errorToken.TokenId + ". NOT IMPLEMENTED!");
                                throw new ArgumentException("\r\nError in I/O Completed, id = " + errorToken.TokenId);
                            }
                    }
                }
                catch(Exception err)
                {
                    var errorToken = (DataHoldingUserToken)e.UserToken;
                    LogManager.GetLogger(this.GetType()).Error("EXCEPTION in I/O Completed, id = " + errorToken.TokenId + ".", err);
                }
            };

            _socket.ReceiveAsync(e);
        }

        private void ProcessReceive(SocketAsyncEventArgs args)
        {
            var receiveSendToken = (DataHoldingUserToken)args.UserToken;

            if (args.SocketError != SocketError.Success)
            {
                LogManager.GetLogger(this.GetType()).Warn("Process Receive ERROR " + args.SocketError.ToString() + ", id " + receiveSendToken.TokenId);
                receiveSendToken.Reset();
                StartDisconnect(args);
                return;
            }

            //If no data was received, close the connection.
            if (args.BytesTransferred == 0)
            {
                LogManager.GetLogger(this.GetType()).Warn("Process Receive NO DATA , id " + receiveSendToken.TokenId);
                receiveSendToken.Reset();
                StartDisconnect(args);
                return;
            }

            LogManager.GetLogger(this.GetType()).Debug("ProcessReceive() Success, id " + receiveSendToken.TokenId + ". Bytes read this op = " + args.BytesTransferred + ".");

            // For debug purposes only
            //This only gives us a readable string if it is operating on string data.
            var tempString = Encoding.ASCII.GetString(args.Buffer, receiveSendToken.receiveMessageOffset, args.BytesTransferred);
            LogManager.GetLogger(this.GetType()).Debug(receiveSendToken.TokenId + " data received = " + tempString);

            var incomingTcpMessageIsReady = _messageHandler.HandleMessage(args, receiveSendToken);

            if (incomingTcpMessageIsReady == true)
            {
                receiveSendToken.onReceive(receiveSendToken.theDataHolder.dataMessageReceived);
                receiveSendToken.theDataHolder.dataMessageReceived = null;
                receiveSendToken.Reset();

                // If there are more bytes on the current message, process them
                if (args.BytesTransferred != receiveSendToken.receiveMessageOffset)
                {
                    ProcessReceive(args);
                }
            }

            receiveSendToken.receiveMessageOffset = 0;
            StartReceive(args);
        }

        //____________________________________________________________________________
        // Set the receive buffer and post a receive op.
        private void StartReceive(SocketAsyncEventArgs receiveSendEventArgs)
        {
            var receiveSendToken = (DataHoldingUserToken)receiveSendEventArgs.UserToken;
            //Set buffer for receive.          
            //receiveSendEventArgs.SetBuffer(receiveSendToken.receiveMessageOffset, ChunkSize);
            receiveSendEventArgs.SetBuffer(0, ChunkSize);

            LogManager.GetLogger(this.GetType()).Debug("\r\nStartReceive, id = " + receiveSendToken.TokenId);

            var willRaiseEvent = _socket.ReceiveAsync(receiveSendEventArgs);
            if (!willRaiseEvent)
            {
                LogManager.GetLogger(this.GetType()).Debug("StartReceive in if (!willRaiseEvent), id = " + receiveSendToken.TokenId);
                ProcessReceive(receiveSendEventArgs);
            }
        }

        //private byte[] messageBuffer;

        //public void StartReceivingMessages(Func<string, bool> onReceiveMessage, string messageSeparator)
        //{
        //    var e = new SocketAsyncEventArgs();
        //    e.SetBuffer(new byte[ChunkSize], 0, ChunkSize);

        //    messageBuffer = new byte[0];

        //    e.Completed += (sender, args) => {

        //        Array.Resize(ref messageBuffer, messageBuffer.Length + args.BytesTransferred); // might be slow
        //        Array.Copy(args.Buffer, messageBuffer, args.BytesTransferred);

        //        onReceive(copyData);

        //        _socket.ReceiveAsync(e);
        //    };
        //    _socket.ReceiveAsync(e);
        //}

        public void Send(string message)
        {
            Send(_encoder.GetBytes(message));
        }

        public void Send(byte[] message)
        {
            _socket.Send(message);
        }

        public void Dispose()
        {
            _socket.Disconnect(true);
            _socket.Dispose();
        }

        //____________________________________________________________________________
        // Disconnect from the host.        
        private void StartDisconnect(SocketAsyncEventArgs receiveSendEventArgs)
        {
            var receiveSendToken = (DataHoldingUserToken)receiveSendEventArgs.UserToken;
            LogManager.GetLogger(this.GetType()).Debug("Disconnecting id: " + receiveSendToken.TokenId);

            _socket.Shutdown(SocketShutdown.Both);
            var willRaiseEvent = this._socket.DisconnectAsync(receiveSendEventArgs);
            if (!willRaiseEvent)
            {
                ProcessDisconnectAndCloseSocket(receiveSendEventArgs);
            }
        }

        //____________________________________________________________________________
        private void ProcessDisconnectAndCloseSocket(SocketAsyncEventArgs receiveSendEventArgs)
        {
            var receiveSendToken = (DataHoldingUserToken)receiveSendEventArgs.UserToken;
            LogManager.GetLogger(this.GetType()).Debug("ProcessDisconnect id: " + receiveSendToken.TokenId);


            if (receiveSendEventArgs.SocketError != SocketError.Success)
            {
                LogManager.GetLogger(this.GetType()).Debug("ProcessDisconnect ERROR, id " + receiveSendToken.TokenId);
            }

            //This method closes the socket and releases all resources, both
            //managed and unmanaged. It internally calls Dispose.
            receiveSendEventArgs.AcceptSocket.Close();

            //create an object that we can write data to.
            receiveSendToken.CreateNewDataHolder();
        }
    }
}
