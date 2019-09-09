// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Utilities
{
	public class PagedResults<T>
	{
		public List<T> Results { get; set; }
		public int TotalResults { get; set; }
	}
}
