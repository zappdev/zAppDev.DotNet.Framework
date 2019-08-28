using System;
using System.Reflection;
using System.Text;

namespace zAppDev.DotNet.Framework.Utilities
{
    public class EncodingUtilities
    {
        public static byte[] StringToByteArray(string data, string encoding)
        {
            var encodingParsed = ParseEncoding(encoding);
            return encodingParsed.GetBytes(data);
        }

        public static string ByteArrayToString(byte[] data, string encoding)
        {
            var encodingParsed = ParseEncoding(encoding);
            return encodingParsed.GetString(data);
        }

        private static Encoding ParseEncoding(string encoding)
        {
            try
            {
                return Encoding.GetEncoding(encoding);
            }
            catch (Exception ex) 
            {
                log4net.LogManager.GetLogger(typeof(Encoding))
                    .Debug($"Could not Parse : {encoding} to Encoding", ex);
                throw new Exception($"Encoding with name '{encoding}' is invalid");
            }

        }
    }
}
