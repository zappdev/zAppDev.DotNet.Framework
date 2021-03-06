﻿// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;

namespace zAppDev.DotNet.Framework.Sockets
{
    public class DataHolder
    {
        internal byte[] dataMessageReceived;
     
        public DataHolder()
        {
        }

        internal void Append(byte[] buffer, int numOfBytesToAppend)
        {
            int prevLength = 0;

            if (dataMessageReceived == null)
            {
                dataMessageReceived = new byte[numOfBytesToAppend];
            }
            else
            {
                // Resize buffer to accommodate new data
                prevLength = dataMessageReceived.Length;
                Array.Resize(ref dataMessageReceived, prevLength + numOfBytesToAppend);
            }

            // Append data
            Buffer.BlockCopy(buffer, 0, dataMessageReceived, prevLength, numOfBytesToAppend);
        }
    }
}
