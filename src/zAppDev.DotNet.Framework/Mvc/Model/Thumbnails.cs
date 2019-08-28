using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace zAppDev.DotNet.Framework.Mvc
{
    public partial class FileHelper
    {
        public class Thumbnails        
        {
            private static log4net.ILog _logger => log4net.LogManager.GetLogger(typeof(Thumbnails));

            private static Dictionary<string, string> ThumbnailCache = new Dictionary<string, string>();

            private static string GetCacheKey(string id, int w, int h)
            {
                return $"{id}#{w}#{h}";
            }

            public static byte[] GetThumbnail(string path, int w, int h, bool useCache = true)
            {
                _logger.Debug($"Requested thumbnail for path: [{path}], width: [{w}], height: [{h}], using cache: [{useCache}]");
                return File.ReadAllBytes(GetThumbnailFullPath(path, w, h, useCache));
            }

            public static byte[] GetThumbnail(Guid? id, byte[] bytes, int w, int h, bool useCache = true)
            {
                _logger.Debug($"Requested thumbnail for file ID: [{id}], width: [{w}], height: [{h}], using cache: [{useCache}]");
                return File.ReadAllBytes(GetThumbnailFullPath(id, bytes, w, h, useCache));
            }
                       
            private static string GetThumbnailFullPath(Guid? id, byte[] bytes, int w, int h, bool useCache)
            {
                var key = GetCacheKey(id.GetValueOrDefault().ToString(), w, h);

                if (!useCache || !ThumbnailCache.ContainsKey(key))
                {
                    _logger.Debug($"Creating thumbnail for file ID: [{id}], width: [{w}], height: [{h}], using cache: [{useCache}]");

                    var tempFilenameOriginal = Path.GetTempFileName();
                    File.WriteAllBytes(tempFilenameOriginal, bytes);

                    var tempFilenameThumb = SaveThumbnailAndGetFullPath(tempFilenameOriginal, w, h);
                    ThumbnailCache[key] = tempFilenameThumb;
                }

                return ThumbnailCache[key];
            }

            private static string GetThumbnailFullPath(string path, int w, int h, bool useCache)
            {
                var key = GetCacheKey(path, w, h);

                if (!useCache || !ThumbnailCache.ContainsKey(key))
                {
                    _logger.Debug($"Creating thumbnail for path: [{path}], width: [{w}], height: [{h}], using cache: [{useCache}]");

                    var tempFilenameThumb = SaveThumbnailAndGetFullPath(path, w, h);
                    ThumbnailCache[key] = tempFilenameThumb;
                }

                return ThumbnailCache[key];
            }

            private static Image GetThumbnailImage(Image image, int w, int h)
            {
                return image.GetThumbnailImage(w, h, () => false, IntPtr.Zero);
            }

            private static string SaveThumbnailAndGetFullPath(string originalImagePath, int w, int h)
            {
                var image = Image.FromFile(originalImagePath);
                
                if (w == 0 && h == 0)
                {
                    _logger.Debug($"Thumbnail requested without dimensions. Autosizing based on image height...");
                    h = image.Height > 960 ? image.Height / 4 : 240;
                }

                if (w > image.Width)
                {
                    _logger.Debug($"Thumbnail width is bigger than original image width. Using original image width...");
                    w = image.Width;
                }

                if (h > image.Height)
                {
                    _logger.Debug($"Thumbnail height is bigger than original image height. Using original image height...");
                    h = image.Height;
                }

                if (w == 0)
                {
                    _logger.Debug($"Thumbnail width not provided, autocalculating based on height ratio...");
                    w = (int)(image.Width / ((double)(image.Height) / h));
                }

                if (h == 0)
                {                    
                    _logger.Debug($"Thumbnail height not provided, autocalculating based on width ratio...");
                    h = (int)(image.Height / ((double)(image.Width) / w));
                }

                var thumbnail = GetThumbnailImage(image, w, h);
                var tempFilename = Path.GetTempFileName();
                thumbnail.Save(tempFilename);

                _logger.Debug($"Thumbnail created in path: [{tempFilename}], width: [{w}], height: [{h}], original image path: [{originalImagePath}]");

                return tempFilename;
            }
        }
    }    
}