using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace zAppDev.DotNet.Framework.Utilities
{
    public class BinarySerializer
    {
        public static byte[] Serialize(object obj)
        {
            if (obj == null)
                return null;
            var bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static object Desserialize(byte[] bytes)
        {
            if (bytes == null)
                return default;
            var bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                var obj = bf.Deserialize(ms);
                return obj;
            }
        }
    }
}
