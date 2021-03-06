﻿// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;

namespace zAppDev.DotNet.Framework.Sockets
{
    class DataHoldingUserToken
    {
        internal DataHolder theDataHolder;

        private string idOfThisObject; //for testing only

        internal int bytesAlreadyRead = 0;
        internal int receiveMessageOffset = 0;
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
