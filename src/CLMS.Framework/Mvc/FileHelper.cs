using System;
using System.IO;
using System.Linq;
using System.Text;

#if NETFRAMEWORK
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
#else
using CLMS.Framework.Utilities;
#endif

namespace CLMS.Framework.Mvc
{
    public partial class FileHelper
    {
        public static string UploadsRelativePath = "~/App_Data/Uploads/";

#if NETFRAMEWORK
        public static string UploadsAbsolutePhysicalPath => HttpContext.Current.Server.MapPath(UploadsRelativePath);
#else
        public static string UploadsAbsolutePhysicalPath => Web.MapPath(UploadsRelativePath);
#endif
        
        private static PendingUploads _pendingUploadsInstance;

        private static PendingDownloads _pendingDownloadsInstance;

        private static log4net.ILog _logger => log4net.LogManager.GetLogger(typeof(FileHelper));

        public static string GetUploadedFilePath(string controller, string ownerID, string path)
        {
            if (File.Exists(path))
            {
                return path;
            }
            else
            {
                var entry = PendingUploadInstance.Get(controller, ownerID);
                if (entry != null && File.Exists(entry.TempPath))
                {
                    return entry.TempPath;
                }
            }

            throw new ApplicationException("The requested File Path has not been found.");
        }
        
        public static void ClearTempData()
        {
#if NETFRAMEWORK
            var tempFolder = HttpContext.Current.Server.MapPath("~/App_Data/temp");
#else
            var tempFolder = Web.MapPath("~/App_Data/temp");
#endif

            if (!Directory.Exists(tempFolder)) return;

            try
            {
                Directory.Delete(tempFolder, true);
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(typeof(FileHelper)).Error("Could not delete temp folder!", e);
            }
        }

        public static PendingUploads PendingUploadInstance
        {
            get
            {
                try
                {
#if NETFRAMEWORK
                    var pendingUpload = HttpContext.Current?.GetOwinContext()?.Get<PendingUploads>();
                    if (pendingUpload != null) return pendingUpload;

                    pendingUpload = new PendingUploads();
                    _pendingUploadsInstance = pendingUpload;
                    HttpContext.Current?.GetOwinContext()?.Set(pendingUpload);
                    return pendingUpload;
#else
                    throw new NotImplementedException();
#endif
                }
                catch (NotImplementedException ex)
                {
                    throw ex;
                }
                catch (Exception)
                {
                    if (_pendingUploadsInstance == null)
                    {
                        throw new ApplicationException("There is no instance of PendingUpload helper!!!");
                    }
                    return _pendingUploadsInstance;
                }
            }
        }

        public static PendingDownloads PendingDownloadInstance
        {
            get
            {
                try
                {
#if NETFRAMEWORK
                    var pendingDowload = HttpContext.Current?.GetOwinContext()?.Get<PendingDownloads>();
                    if (pendingDowload != null) return pendingDowload;

                    pendingDowload = new PendingDownloads();
                    _pendingDownloadsInstance = pendingDowload;
                    HttpContext.Current?.GetOwinContext()?.Set(pendingDowload);
                    return pendingDowload;
#else
                    throw new NotImplementedException();
#endif
                }
                catch (NotImplementedException ex)
                {
                    throw ex;
                }
                catch (Exception)
                {
                    if (_pendingDownloadsInstance == null)
                    {
                        throw new ApplicationException("There is no instance of PendingDownload helper!!!");
                    }
                    return _pendingDownloadsInstance;
                }
            }
        }
        
        public static byte[] GetStoredFileContents(Func<object, int[], string> relativePathFunc, object viewModel, int[] indexes)
        {
            var file = GetFileByEvaluatedFileNameExpression(relativePathFunc, viewModel, indexes);
            return File.ReadAllBytes(file.FullName);
        }

        public static string DownloadFromFileSystemAsBase64(Func<object, int[], string> relativePathFunc, object viewModel, int[] indexes)
        {
            var contents = GetStoredFileContents(relativePathFunc, viewModel, indexes);
            return Convert.ToBase64String(contents);
        }

        public static FileInfo GetFileByEvaluatedFileNameExpression(Func<object, int[], string> relativePathFunc, object viewModel, int[] indexes, bool throwIfNotFound = false)
        {
            var relativePath = relativePathFunc.Invoke(viewModel, indexes);
            relativePath = relativePath.Replace("/", "\\");

            var absolutePath = Path.Combine(UploadsAbsolutePhysicalPath, relativePath);
            var parentFolderInfo = new FileInfo(absolutePath).Directory;
            var fileName = Path.GetFileName(absolutePath);
            var file = parentFolderInfo?.GetFiles(fileName).FirstOrDefault();

            if (file == null)
            {
                _logger.Error($"Could not find stored file '{absolutePath}' (extension is autodetected)");

                if (throwIfNotFound)
                {
                    throw new FileNotFoundException("Not found stored file!", absolutePath);
                }
            }

            return file;
        }

        public static bool IsRelativePathValid(string path, bool throwException = true)
        {
            var valid = false;

            if (path.Contains("..") || path.Contains(":") || path.StartsWith("\\"))
            {
                valid = false;

                if (throwException)
                {
                    throw new ApplicationException($"Path '{path}' contains invalid characters");
                }
            }

            return valid;
        }

        public static string GetUtf8SafeFilename(string fileName)
        {                        
            if (string.IsNullOrWhiteSpace(fileName)) return null;

            var utfMark = "=?utf-8?B?";

            // https://stackoverflow.com/questions/10828807/how-to-decode-utf-8b-to-string-in-c-sharp
            if (fileName.StartsWith(utfMark))
            {
                var startIndex = fileName.IndexOf(utfMark) + utfMark.Length;
                var stopIndex = fileName.LastIndexOf("?=");
                var partLength = stopIndex - startIndex;
                var namePart = fileName.Substring(startIndex, partLength);
                var bytez = Convert.FromBase64String(namePart);

                fileName = Encoding.UTF8.GetString(bytez);
            }

            return Path.GetFileName(fileName);
        }
    }    
}