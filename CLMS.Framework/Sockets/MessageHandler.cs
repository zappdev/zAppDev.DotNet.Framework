using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace CLMS.Framework.Sockets
{
    class MessageHandler
    {
        private static int SearchBytes(byte[] haystack, byte[] needle)
        {
            var len = needle.Length;
            var limit = haystack.Length - len;
            for (var i = 0; i <= limit; i++)
            {
                var k = 0;
                for (; k < len; k++)
                {
                    if (needle[k] != haystack[i + k]) break;
                }
                if (k == len) return i;
            }
            return -1;
        }

        public bool HandleMessage(SocketAsyncEventArgs receiveSendEventArgs, DataHoldingUserToken receiveSendToken)
        {
            var logger = LogManager.GetLogger(this.GetType());
            bool incomingTcpMessageIsReady = false;

            var currentChunk = new byte[receiveSendEventArgs.BytesTransferred];
            Array.Copy(receiveSendEventArgs.Buffer, currentChunk, receiveSendEventArgs.BytesTransferred);

            // Look for delimiter in bytes
            var delimiterPos = SearchBytes(currentChunk, receiveSendToken.messageDelimiter);

            if(delimiterPos < 0)
            {
                logger.Debug("No delimiter yet for token: " + receiveSendToken.TokenId);
                // No delimiter. We need to receive more data...
                // Append data to the token buffer
                receiveSendToken.theDataHolder.Append(receiveSendEventArgs.Buffer, receiveSendEventArgs.BytesTransferred);
                receiveSendToken.bytesAlreadyRead += receiveSendEventArgs.BytesTransferred;
                receiveSendToken.receiveMessageOffset = 0;
                // receive more...
            }
            else
            {
                logger.Debug("Found the end of a message! Token: " + receiveSendToken.TokenId);
                receiveSendToken.theDataHolder.Append(receiveSendEventArgs.Buffer, delimiterPos); // read up until before the delimiter
                receiveSendToken.bytesAlreadyRead += delimiterPos + receiveSendToken.messageDelimiter.Length - 1;
                receiveSendToken.receiveMessageOffset = delimiterPos + receiveSendToken.messageDelimiter.Length;
                incomingTcpMessageIsReady = true;
            }

            return incomingTcpMessageIsReady;
        }
    }
}
