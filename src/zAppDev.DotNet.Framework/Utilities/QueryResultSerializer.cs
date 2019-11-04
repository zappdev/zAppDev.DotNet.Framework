using System;
using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Utilities
{
    public class QueryResultSerializer<T>
    {
        public List<T> Serialize(List<Dictionary<string, object>> qResponse)
        {
            var entryType = typeof(T);
            var props = entryType.GetProperties();

            var data = new List<T>();

            foreach (var line in qResponse)
            {
                var entry = Activator.CreateInstance<T>();

                foreach (var prop in props)
                {
                    if (!line.ContainsKey(prop.Name))
                    {
                        continue;
                    }

                    var val = line[prop.Name];
                    if (val is System.DBNull)
                    {
                        val = null;
                    }

                    prop.SetValue(entry, val);
                }

                data.Add(entry);
            }

            return data;
        }
    }
}
