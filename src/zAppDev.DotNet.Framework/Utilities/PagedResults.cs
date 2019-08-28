using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Utilities
{
	public class PagedResults<T>
	{
		public List<T> Results { get; set; }
		public int TotalResults { get; set; }
	}
}
