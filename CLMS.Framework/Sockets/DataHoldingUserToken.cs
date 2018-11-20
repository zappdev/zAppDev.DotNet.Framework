using System;
using System.Net.Sockets;
using System.Text;

namespace CLMS.Framework.Sockets
{
    class DataHoldingUserToken
    {
        internal DataHolder theDataHolder;

        private string idOfThisObject; //for testing only

        internal Int32 bytesAlreadyRead = 0;
        internal Int32 receiveMessageOffset = 0;
        internal byte[] messageDelimiter;
        public Func<byte[], bool> onReceive;

        public DataHoldingUserToken(byte[] delimiter, string identifier, Func<byte[], bool> onReceive)
        {
            this.messageDelimiter = delimiter;
            this.idOfThisObject = identifier;
            this.onReceive = onReceive;
        }

        public string TokenId
        {
            get
            {
                return idOfThisObject;
            }
        }

        internal void CreateNewDataHolder()
        {
            theDataHolder = new DataHolder();
        }

        public void Reset()
        {
            this.bytesAlreadyRead = 0;
            //this.receivedPrefixBytesDoneCount = 0;
            //this.receivedMessageBytesDoneCount = 0;
            //this.recPrefixBytesDoneThisOp = 0;
            //this.receiveMessageOffset = this.permanentReceiveMessageOffset;
        }
    }
}
