using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Mvc
{
    public class ChartHelper
    {
        public class ChartResult
        {
			public ChartResult() 
			{
				Values = new List<object>();
				ValueLabels = new List<object>();
			}
			
            public ChartResult(object item, object label, List<object> values, List<object> valueLabels, List<object> radius = null)
            {
                Item = item;
                Label = label;
                Values = values;
                ValueLabels= valueLabels;
                Radius = radius;
            }

            public object Item { get; set; }
            public object Label { get; set; }            
            public List<object> Values { get; set; }
			public List<object> ValueLabels { get; set; }
            public List<object> Radius { get; set; }
        }
    }
}