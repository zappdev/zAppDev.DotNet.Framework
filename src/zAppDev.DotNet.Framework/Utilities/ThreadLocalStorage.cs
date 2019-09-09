// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using System.Threading;

namespace zAppDev.DotNet.Framework.Utilities
{
	public class ThreadLocalStorage
	{
		private static readonly ThreadLocal<Dictionary<string, object>> Items 
			= new ThreadLocal<Dictionary<string, object>>(() => new Dictionary<string, object>());

		public static object Get<T>(string key)
		{
			return Items.Value.ContainsKey(key) ? (T)Items.Value[key] : default(T);
		}

		public static void Remove(string key)
		{
			if (Items.Value.ContainsKey(key))
			{
				Items.Value.Remove(key);
			}
		}

		public static void Set(string key, object obj)
		{
			if (Items.Value.ContainsKey(key))
			{
				// Update
				Items.Value[key] = obj;
			}
			else
			{
				Items.Value.Add(key, obj);
			}
		}
	}
}