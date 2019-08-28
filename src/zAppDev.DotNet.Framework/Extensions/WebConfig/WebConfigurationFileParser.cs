#if NETFRAMEWORK
#else
using System;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace zAppDev.DotNet.Framework.Extensions.WebConfig
{
    internal class WebConfigurationFileParser
    {
        private readonly IDictionary<string, string> _data =
            new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly Stack<string> _context = new Stack<string>();
        private readonly Stack<string> _openElement = new Stack<string>();

        public IDictionary<string, string> Parse(Stream stream)
        {
            _data.Clear();
            _context.Clear();

            var settings = new XmlReaderSettings { Async = false };

            using (var reader = XmlReader.Create(stream, settings))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:                            
                            VisitNode(reader);
                            break;
                        case XmlNodeType.EndElement:
                            var closedElement = _openElement.Pop();
                            while (!_context.Pop().Equals(closedElement))
                            {
                                
                            }

                            break;
                        default:
                            break;
                    }
                }
            }

            return _data;
        }

        // Final 'leaf' call for each tree which records the setting's value 
        private void VisitNode(XmlReader reader)
        {
            var name = reader.Name;
            if (reader.IsEmptyElement)
            {
                _context.Push(name);
                VisitAttributes(reader);
                _context.Pop();
            }
            else
            {
                _context.Push(name);
                _openElement.Push(name);
                var key = VisitAttributes(reader);
                if (!string.IsNullOrEmpty(key))
                {
                    _context.Push(key);
                }
            }
        }

        private string VisitAttributes(XmlReader reader)
        {
            var key = "";
            var values = new Dictionary<string, string>();

            for (var attInd = 0; attInd < reader.AttributeCount; attInd++)
            {
                reader.MoveToAttribute(attInd);


                switch (reader.Name)
                {
                    case "key":
                    case "id":
                    case "name":
                    case "region":
                    case "path":
                    case "assembly":
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

            return key;
        }

        private string GetCurrentPath() => string.Join(":", _context.Reverse().ToList());
    }
}
#endif