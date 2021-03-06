﻿// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Text;

namespace zAppDev.DotNet.Framework.Utilities
{
    public class ZipIonic
    {
        public static string Compress(string str)
        {
            try
            {
                return Convert.ToBase64String(CompressToBytes(str));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string Decompress(string str)
        {
            try
            {
                return DecompressFromBytes(Convert.FromBase64String(str));
            }
            catch (Exception)
            {
                return null;
            }

        }


        public static byte[] CompressToBytes(string str)
        {
            try
            {
                return (Ionic.Zlib.ZlibStream.CompressBuffer(Encoding.UTF8.GetBytes(str)));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string DecompressFromBytes(byte[] bytes)
        {
            try
            {
                return Encoding.UTF8.GetString(Ionic.Zlib.ZlibStream.UncompressBuffer(bytes));
            }
            catch (Exception)
            {
                return null;
            }

        }
    }
}
