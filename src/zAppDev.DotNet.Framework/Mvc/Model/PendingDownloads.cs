using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

#if NETFRAMEWORK
using System.Web.Mvc;
#else
using zAppDev.DotNet.Framework.Utilities;
using Microsoft.AspNetCore.Mvc;
#endif

namespace zAppDev.DotNet.Framework.Mvc
{
    public partial class FileHelper
    {
        public class PendingDownloads
        {
            public Dictionary<string, string> GetPendingDownloads(string controller)
            {
#if NETFRAMEWORK
                var key = $"{HttpContext.Current.Session.SessionID}{controller}_PendingDownloads";
                return CLMS.AppDev.Cache.CacheManager.Current.Get(key, new Dictionary<string, string>());
#else
                var key = $"{controller}_PendingDownloads";
                return Web.Session.Get(key, new Dictionary<string, string>());
#endif
            }

            public void UpdatePendingDownloads(string controller, Dictionary<string, string> value)
            {
#if NETFRAMEWORK
                var key = $"{HttpContext.Current.Session.SessionID}{controller}_PendingDownloads";
                CLMS.AppDev.Cache.CacheManager.Current.Set(key, value);
#else
                var key = $"{controller}_PendingDownloads";
                Web.Session.Add(key, value);
#endif
            }

            // DB stored
            public string Add(string controller, byte[] content, string fileName)
            {
                var key = Guid.NewGuid().ToString().Replace("-", "");

                // add key to path, to ensure no conflicts occure
                // when more than 1 users request the same file at the same time
                var tempFolderAbsolutePath = Path.Combine(zAppDev.DotNet.Framework.Utilities.Common.GetApplicationTempFolderPath(), key);

                // At the extremly rare event that this directory exists, delete
                if (Directory.Exists(tempFolderAbsolutePath))
                {
                    Directory.Delete(tempFolderAbsolutePath, true);
                }

                // Create random directory
                Directory.CreateDirectory(tempFolderAbsolutePath);

                var absolutePath = Path.Combine(tempFolderAbsolutePath, fileName);

                File.WriteAllBytes(absolutePath, content);

                var currentValue = GetPendingDownloads(controller);
                currentValue.Add(key, absolutePath);
                UpdatePendingDownloads(controller, currentValue);

                return key;
            }

            // File System stored
            public string Add(string controller, string relativePath)
            {
                var absolutePath = Path.Combine(UploadsAbsolutePhysicalPath, relativePath);
                var content = File.ReadAllBytes(absolutePath);
                var fileName = Path.GetFileName(absolutePath);
                var key = Guid.NewGuid().ToString().Replace("-", "");
                var currentValue = GetPendingDownloads(controller);
                currentValue.Add(key, absolutePath);
                UpdatePendingDownloads(controller, currentValue);
                return key;
            }

            [Obsolete("For Legacy Support")]
            public string Add(string controller, Func<object, int[], string> relativePathFunc, object viewModel, int[] indexes)
            {
                var file = GetFileByEvaluatedFileNameExpression(relativePathFunc, viewModel, indexes);

                var content = File.ReadAllBytes(file.FullName);
                var fileName = Path.GetFileName(file.FullName);
                var key = Guid.NewGuid().ToString().Replace("-", "");

                var currentValue = GetPendingDownloads(controller);
                currentValue.Add(key, file.FullName);
                UpdatePendingDownloads(controller, currentValue);

                return key;
            }

            public string GetFilePathByKey(string controller, string key)
            {
                var currentPending = GetPendingDownloads(controller);

                if (GetPendingDownloads(controller).ContainsKey(key))
                {
                    var absolutePath = currentPending[key];
                    return absolutePath;
                }
                else
                {
                    throw new ApplicationException($"Could not find a prepared download with id '{key}'");
                }
            }

            public FileContentResult DownloadByKey(string controller, string key)
            {
                var currentPending = GetPendingDownloads(controller);

                if (GetPendingDownloads(controller).ContainsKey(key))
                {
                    var absolutePath = currentPending[key];
                    var filename = Path.GetFileName(absolutePath);
                    var contents = File.ReadAllBytes(absolutePath);
#if NETFRAMEWORK
                    var username = HttpContext.Current?.Request?.GetOwinContext()?.Request?.User?.Identity?.Name;
                    log4net.LogManager.GetLogger(typeof(FileHelper)).Info($"Download - User [{username}] downloaded file [{absolutePath}]");
#else
#endif
                    currentPending.Remove(key);

                    return new FileContentResult(contents, "application/octet-stream")
                    {
                        FileDownloadName = GetValidDownloadName(filename)
                    };
                }
                else
                {
                    throw new ApplicationException($"Could not find a prepared download with id '{key}'");
                }
            }

            public FileContentResult DownloadByPath(string path, string defaultPath = "")
            {
                IsRelativePathValid(path);

                var absolutePath = Path.Combine(UploadsAbsolutePhysicalPath, path);
#if NETFRAMEWORK
                var username = HttpContext.Current?.Request?.GetOwinContext()?.Request?.User?.Identity?.Name;
#endif

                if (File.Exists(absolutePath))
                {
                    var filename = Path.GetFileName(absolutePath);
                    var contents = File.ReadAllBytes(absolutePath);
#if NETFRAMEWORK
                    log4net.LogManager.GetLogger(typeof(FileHelper)).Info($"Download - User [{username}] downloaded file [{absolutePath}]");
#else
                    log4net.LogManager.GetLogger(typeof(FileHelper)).Info($"Download - Downloaded file [{absolutePath}]");
#endif

                    return new FileContentResult(contents, "application/octet-stream")
                    {
                        FileDownloadName = GetValidDownloadName(filename)
                    };
                }

                if (!string.IsNullOrWhiteSpace(defaultPath))
                {
#if NETFRAMEWORK
                    var defaultAbsolutePath = HttpContext.Current.Server.MapPath(defaultPath);
#else
                    var defaultAbsolutePath = Web.MapPath(defaultPath);
#endif
                    if (File.Exists(defaultAbsolutePath))
                    {
                        var filename = Path.GetFileName(defaultAbsolutePath);
                        var contents = File.ReadAllBytes(defaultAbsolutePath);

#if NETFRAMEWORK
                        log4net.LogManager.GetLogger(typeof(FileHelper)).Info($"Download - User [{username}] downloaded file [{absolutePath}]");
#else
                        log4net.LogManager.GetLogger(typeof(FileHelper)).Info($"Download - Downloaded file [{absolutePath}]");
#endif

                        return new FileContentResult(contents, "application/octet-stream")
                        {
                            FileDownloadName = GetValidDownloadName(filename)
                        };
                    }
                }

                throw new ApplicationException($"Could not find file '{path}'");
            }


            public void Clear(string controller)
            {
                UpdatePendingDownloads(controller, new Dictionary<string, string>());
            }
            private string GetValidDownloadName(string fileName)
            {
                return fileName.All(c => c < sbyte.MaxValue)
                    ? fileName
                    : HttpUtility.UrlEncode(fileName).Replace("+", " "); // utf8 characters need encoding
            }
        }
    }    
}