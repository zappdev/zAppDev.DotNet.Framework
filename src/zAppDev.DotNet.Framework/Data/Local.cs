// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections;

namespace zAppDev.DotNet.Framework.Data
{
    public static class Local
    {
        public static LocalData Data { get; } = new LocalData();
    }

    public class LocalData
    {
        [ThreadStatic]
        private static Hashtable _localData;
        private static readonly object LocalDataHashtableKey = new object();

        private static Hashtable LocalHashtable
        {
            get
            {
                if (!RunningInWeb)
                {
                    return _localData ?? (_localData = new Hashtable());
                }

                var context = Utilities.Web.GetContext();
                if (!(context.Items[LocalDataHashtableKey] is Hashtable web_hashtable))
                {
                    web_hashtable = new Hashtable();
                    context.Items[LocalDataHashtableKey] = web_hashtable;
                }
                return web_hashtable;
            }
        }

        public object this[object key]
        {
            get => LocalHashtable[key];
            set => LocalHashtable[key] = value;
        }

        public int Count => LocalHashtable.Count;

        public void Clear()
        {
            LocalHashtable.Clear();
        }

        public static bool RunningInWeb
        {
            get
            {
                return Utilities.Web.GetContext() != null;
            }
        }
    }
}