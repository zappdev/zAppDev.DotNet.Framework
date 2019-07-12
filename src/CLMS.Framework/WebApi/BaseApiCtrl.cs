#if NETFRAMEWORK
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Identity = CLMS.Framework.Identity;
using CLMS.Framework.Identity;

namespace cfTests.Web
{
    public class BasicApiCtrl : ApiController
    {
        protected HttpResponseMessage SendString(int? intStr)
        {
            return SendString(intStr.HasValue ? intStr.Value.ToString() : "0");
        }

        protected HttpResponseMessage SendString(int intStr)
        {
            return SendString(intStr.ToString());
        }

        protected HttpResponseMessage SendString(bool? boolStr)
        {
            return SendString(boolStr.HasValue ? boolStr.Value.ToString().ToLower() : "false");
        }

        protected HttpResponseMessage SendString(bool boolStr)
        {
            return SendString(boolStr.ToString().ToLower());
        }

        protected HttpResponseMessage SendString(string str)
        {
            return new HttpResponseMessage()
            {
                Content = new StringContent(
                    str,
                    System.Text.Encoding.UTF8,
                    "text/html"
                )
            };
        }

        protected HttpResponseMessage SendFile(FileInfo file)
        {
            using (var stream = new MemoryStream(File.ReadAllBytes(file.FullName)))
            {
                var content = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(stream)
                };
                content.Content.Headers.ContentType = new MediaTypeHeaderValue(GetFileContentType(file.Name));
                content.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = file.Name
                };
                return content;
            }
        }

        private string GetFileContentType(string filename)
        {
            Dictionary<string, List<string>> contentTypes = new Dictionary<string, List<string>>
            {
                { "application/andrew-inset", new List<string> { "ez" } },
                { "application/mac-binhex40", new List<string> { "hqx" } },
                { "application/mac-compactpro", new List<string> { "cpt" } },
                { "application/mathml+xml", new List<string> { "mathml" } },
                { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", new List<string> { "docx" } },
                { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new List<string> { "xlsx" } },
                { "application/msword", new List<string> { "doc" } },
                { "application/octet-stream", new List<string> { "bin", "dms", "lha", "lzh", "exe", "class", "so", "dll" } },
                { "application/oda", new List<string> { "oda" } },
                { "application/ogg", new List<string> { "ogg" } },
                { "application/pdf", new List<string> { "pdf" } },
                { "application/postscript", new List<string> { "ai", "eps", "ps" } },
                { "application/rdf+xml", new List<string> { "rdf" } },
                { "application/smil", new List<string> { "smi", "smil" } },
                { "application/srgs", new List<string> { "gram" } },
                { "application/srgs+xml", new List<string> { "grxml" } },
                { "application/vnd.mif", new List<string> { "mif" } },
                { "application/vnd.mozilla.xul+xml", new List<string> { "xul" } },
                { "application/vnd.ms-excel", new List<string> { "xls" } },
                { "application/vnd.ms-powerpoint", new List<string> { "ppt" } },
                { "application/vnd.wap.wbxml", new List<string> { "wbxml" } },
                { "application/vnd.wap.wmlc", new List<string> { "wmlc" } },
                { "application/vnd.wap.wmlscriptc", new List<string> { "wmlsc" } },
                { "application/voicexml+xml", new List<string> { "vxml" } },
                { "application/x-bcpio", new List<string> { "bcpio" } },
                { "application/x-cdlink", new List<string> { "vcd" } },
                { "application/x-chess-pgn", new List<string> { "pgn" } },
                { "application/x-cpio", new List<string> { "cpio" } },
                { "application/x-csh", new List<string> { "csh" } },
                { "application/x-director", new List<string> { "dcr", "dir", "dxr" } },
                { "application/x-dvi", new List<string> { "dvi" } },
                { "application/x-futuresplash", new List<string> { "spl" } },
                { "application/x-gtar", new List<string> { "gtar" } },
                { "application/x-hdf", new List<string> { "hdf" } },
                { "application/x-httpd-php", new List<string> { "php", "php4", "php3", "phtml" } },
                { "application/x-httpd-php-source", new List<string> { "phps" } },
                { "application/x-javascript", new List<string> { "js" } },
                { "application/x-koan", new List<string> { "skp", "skd", "skt", "skm" } },
                { "application/x-latex", new List<string> { "latex" } },
                { "application/x-netcdf", new List<string> { "nc", "cdf" } },
                { "application/x-pkcs7-crl", new List<string> { "crl" } },
                { "application/x-sh", new List<string> { "sh" } },
                { "application/x-shar", new List<string> { "shar" } },
                { "application/x-shockwave-flash", new List<string> { "swf" } },
                { "application/x-stuffit", new List<string> { "sit" } },
                { "application/x-sv4cpio", new List<string> { "sv4cpio" } },
                { "application/x-sv4crc", new List<string> { "sv4crc" } },
                { "application/x-tar", new List<string> { "tgz", "tar" } },
                { "application/x-tcl", new List<string> { "tcl" } },
                { "application/x-tex", new List<string> { "tex" } },
                { "application/x-texinfo", new List<string> { "texinfo", "texi" } },
                { "application/x-troff", new List<string> { "t", "tr", "roff" } },
                { "application/x-troff-man", new List<string> { "man" } },
                { "application/x-troff-me", new List<string> { "me" } },
                { "application/x-troff-ms", new List<string> { "ms" } },
                { "application/x-ustar", new List<string> { "ustar" } },
                { "application/x-wais-source", new List<string> { "src" } },
                { "application/x-x509-ca-cert", new List<string> { "crt" } },
                { "application/xhtml+xml", new List<string> { "xhtml", "xht" } },
                { "application/xml", new List<string> { "xml", "xsl" } },
                { "application/xml-dtd", new List<string> { "dtd" } },
                { "application/xslt+xml", new List<string> { "xslt" } },
                { "application/zip", new List<string> { "zip" } },
                { "audio/basic", new List<string> { "au", "snd" } },
                { "audio/midi", new List<string> { "mid", "midi", "kar" } },
                { "audio/mpeg", new List<string> { "mpga", " mp2", "mp3" } },
                { "audio/x-aiff", new List<string> { "aif", "aiff", "aifc" } },
                { "audio/x-mpegurl", new List<string> { "m3u" } },
                { "audio/x-pn-realaudio", new List<string> { "ram", "rm" } },
                { "audio/x-pn-realaudio-plugin", new List<string> { "rpm" } },
                { "audio/x-realaudio", new List<string> { "ra" } },
                { "audio/x-wav", new List<string> { "wav" } },
                { "chemical/x-pdb", new List<string> { "pdb" } },
                { "chemical/x-xyz", new List<string> { "xyz" } },
                { "image/bmp", new List<string> { "bmp" } },
                { "image/cgm", new List<string> { "cgm" } },
                { "image/gif", new List<string> { "gif" } },
                { "image/ief", new List<string> { "ief" } },
                { "image/jpeg", new List<string> { "jpeg", "jpg", "jpe" } },
                { "image/png", new List<string> { "png" } },
                { "image/svg+xml", new List<string> { "svg" } },
                { "image/tiff", new List<string> { "tiff", "tif" } },
                { "image/vnd.djvu", new List<string> { "djvu", "djv" } },
                { "image/vnd.wap.wbmp", new List<string> { "wbmp" } },
                { "image/x-cmu-raster", new List<string> { "ras" } },
                { "image/x-icon", new List<string> { "ico" } },
                { "image/x-portable-anymap", new List<string> { "pnm" } },
                { "image/x-portable-bitmap", new List<string> { "pbm" } },
                { "image/x-portable-graymap", new List<string> { "pgm" } },
                { "image/x-portable-pixmap", new List<string> { "ppm" } },
                { "image/x-rgb", new List<string> { "rgb" } },
                { "image/x-xbitmap", new List<string> { "xbm" } },
                { "image/x-xpixmap", new List<string> { "xpm" } },
                { "image/x-xwindowdump", new List<string> { "xwd" } },
                { "model/iges", new List<string> { "igs", "iges" } },
                { "model/mesh", new List<string> { "msh", "mesh", "silo" } },
                { "model/vrml", new List<string> { "wrl", "vrml" } },
                { "text/calendar", new List<string> { "ics", "ifb" } },
                { "text/css", new List<string> { "css" } },
                { "text/html", new List<string> { "shtml", "html", "htm" } },
                { "text/plain", new List<string> { "asc", "txt" } },
                { "text/richtext", new List<string> { "rtx" } },
                { "text/rtf", new List<string> { "rtf" } },
                { "text/sgml", new List<string> { "sgml", "sgm" } },
                { "text/tab-separated-values", new List<string> { "tsv" } },
                { "text/vnd.wap.wml", new List<string> { "wml" } },
                { "text/vnd.wap.wmlscript", new List<string> { "wmls" } },
                { "text/x-setext", new List<string> { "etx" } },
                { "video/mpeg", new List<string> { "mpeg", "mpg", "mpe" } },
                { "video/quicktime", new List<string> { "qt", "mov" } },
                { "video/vnd.mpegurl", new List<string> { "mxu" } },
                { "video/x-msvideo", new List<string> { "avi" } },
                { "video/x-sgi-movie", new List<string> { "movie" } },
                { "x-conference/x-cooltalk", new List<string> { "ice" } }
            };
            string ext = filename.Substring(filename.LastIndexOf(".") + 1);
            var types = contentTypes.Where(type => type.Value.Contains(ext)).ToList();
            return types.Any() ? types.First().Key : "application/octet-stream";
        }
    }
}
#endif