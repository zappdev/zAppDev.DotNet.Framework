#if NETFRAMEWORK
#else
using System;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CLMS.Framework.Extensions.WebConfig
{
    internal class WebConfigurationFileParser
    {
        private readonly IDictionary<string, string> _data =
            new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly Stack<string> _context = new Stack<string>();

        public async Task<IDictionary<string, string>> Parse(Stream stream)
        {
            _data.Clear();
            _context.Clear();

            var settings = new XmlReaderSettings { Async = true };

            using (var reader = XmlReader.Create(stream, settings))
            {
                while (await reader.ReadAsync())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            VisitNode(reader);
                            break;
                        case XmlNodeType.EndElement:
                            Console.WriteLine("End Element {0} {1}", reader.Name, reader.Value);
                            _context.Pop();
                            break;
                        default:
                            Console.WriteLine("Other node {0} with value {1}", reader.NodeType, reader.Value);
                            break;
                    }
                }
            }

            return _data;
        }

        // Final 'leaf' call for each tree which records the setting's value 
        private void VisitNode(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                _context.Push(reader.Name);

                var key = "";
                var values = new Dictionary<string, string>();

                for (var attInd = 0; attInd < reader.AttributeCount; attInd++)
                {
                    reader.MoveToAttribute(attInd);


                    switch (reader.Name)
                    {
                        case "key":
                        case "name":
                            key = reader.Value;
                            break;
                        default:
                            values.Add(reader.Name, reader.Value);
                            break;
                    }                    
                }

                foreach (var value in values)
                {
                    var path = (string.IsNullOrEmpty(key))
                        ? $"{GetCurrentPath()}:{value.Key}"
                        : $"{GetCurrentPath()}:{key}:{value.Key}";
                    _data.Add(path, value.Value);
                }

                _context.Pop();
            }
            else
            {
                _context.Push(reader.Name);
            }
        }

        private string GetCurrentPath() => string.Join(":", _context.Reverse().ToList());
    }
}
#endif