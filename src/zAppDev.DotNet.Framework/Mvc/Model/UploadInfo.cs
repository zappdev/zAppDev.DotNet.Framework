// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Globalization;
using System.IO;
using System.Linq;

#if NETFRAMEWORK
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif

namespace zAppDev.DotNet.Framework.Mvc
{
    public partial class FileHelper
    {
        public class UploadInfo
        {
            public Func<object, int[], string> GetRelativePath { get; set; }
            public Action<object, byte[], int[]> UpdateBinaryField { get; set; }
            public Action<object, byte[], string, int[]> UpdateBinding { get; set; }
            public byte[] BinaryContents => File.ReadAllBytes(TempPath);
            public int[] Indexes { get; set; }
            public string OwnerControlId { get; set; }
            public string TempPath { get; set; }
            public string Controller { get; set; }
            public bool IsWrittenToFileSystem => UpdateBinaryField == null;
            public string Key => $"{OwnerControlId}_{string.Join(";", Indexes ?? new int[] { })}";
            public string FileName { get; set; }
            public bool IsLegacy => UpdateBinding == null;

#if NETFRAMEWORK
            public UploadInfo(HttpPostedFileBase file, string controller, string ownerId, int[] indexes)
#else
            public UploadInfo(IFormFile file, string controller, string ownerId, int[] indexes)
#endif
            {
                Controller = controller;
                OwnerControlId = ownerId;
                Indexes = indexes;
                var fileName = GetUtf8SafeFilename(file?.FileName);
                FileName = fileName;
                TempPath = Path.Combine(zAppDev.DotNet.Framework.Utilities.Common.GetApplicationTempFolderPath(), fileName + "_" + Guid.NewGuid() + ".tmp");

                WriteFileTemporary(file);
            }

            public void Commit(object viewModel, bool keepOldCopy = true)
            {
                if (GetRelativePath != null)
                {
                    WriteFile(viewModel, keepOldCopy); // File System
                }
                else
                {
                    UpdateDbBound(viewModel); //DB
                }

                TryToDeleteTempFile();
            }

            public string ToBase64String()
            {
                return Convert.ToBase64String(BinaryContents);
            }

            [Obsolete("For Legacy Support")]
            private void UpdateBoundBinaryField(object viewModel)
            {
                UpdateBinaryField.Invoke(viewModel, BinaryContents, Indexes);
            }

            private void UpdateDbBound(object viewModel)
            {
#if NETFRAMEWORK
                var username = HttpContext.Current?.Request?.GetOwinContext()?.Request?.User?.Identity?.Name;
                _logger.Info($"Upload - User [{username}] uploaded DB File with id [{Key}]");
#else
                _logger.Info($"Upload - Uploaded DB File with id [{Key}]");
#endif

                if (IsLegacy)
                {
                    UpdateBoundBinaryField(viewModel);
                }
                else
                {
                    UpdateBinding?.Invoke(viewModel, BinaryContents, FileName, Indexes);
                }
            }

            private void WriteFile(object viewModel, bool keepOldCopy)
            {
                var relativePath = GetRelativePath.Invoke(viewModel, Indexes);

                if (keepOldCopy)
                {
                    StoreOldCopyOfFile(relativePath);
                }

                var absolutePath = Path.Combine(UploadsAbsolutePhysicalPath, relativePath);
                var parentFolderInfo = new FileInfo(absolutePath).Directory;

                if (parentFolderInfo != null && !parentFolderInfo.Exists)
                {
                    parentFolderInfo.Create();
                }

                File.WriteAllBytes(absolutePath, BinaryContents);

                UpdateBinding?.Invoke(viewModel, null, relativePath, Indexes);

#if NETFRAMEWORK
                var username = HttpContext.Current?.Request?.GetOwinContext()?.Request?.User?.Identity?.Name;
                _logger.Info($"Upload - User [{username}] uploaded File [{absolutePath}]");
#else
                _logger.Info($"Upload - Uploaded File [{absolutePath}]");
#endif
            }

#if NETFRAMEWORK
            private void WriteFileTemporary(HttpPostedFileBase file)
            {
                byte[] bytes;

                using (var binaryReader = new BinaryReader(file.InputStream))
                {
                    bytes = binaryReader.ReadBytes(file.ContentLength);
                }

                File.WriteAllBytes(TempPath, bytes);
            }
#else
            private void WriteFileTemporary(IFormFile file)
            {
                using (var outStream = new FileStream(TempPath, FileMode.Create))
                {
                    file.CopyTo(outStream);
                }
            }
#endif

            private readonly object _fileLock = new object();
            private void StoreOldCopyOfFile(string relativePath)
            {
                var absolutePath = Path.Combine(UploadsAbsolutePhysicalPath, relativePath);

                lock (_fileLock)
                {
                    var parentFolderInfo = new FileInfo(absolutePath).Directory;

                    if (parentFolderInfo != null && !parentFolderInfo.Exists) return;

                    var fileName = Path.GetFileName(relativePath);

                    var oldFile = parentFolderInfo?.GetFiles(fileName).FirstOrDefault();

                    if (oldFile == null) return;

                    var oldFileWriteTime = oldFile.LastWriteTimeUtc;
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(relativePath);

                    int backupNumber = 0;

                    var previousBackups = parentFolderInfo?.GetFiles($"{fileNameWithoutExtension}.old_*{oldFile.Extension}");
                    if (previousBackups.Any())
                    {
                        var lastBackupNumberString =
                            previousBackups
                            ?.Select(x => x.Name?.Replace(fileNameWithoutExtension, "")?.Replace(oldFile.Extension, "")?.Replace(".old_", ""))
                            ?.OrderByDescending(x => x)
                            ?.FirstOrDefault();
                        if (int.TryParse(lastBackupNumberString, out int lastBackupNumberInt))
                            backupNumber = lastBackupNumberInt;
                    }

                    backupNumber++;

                    var newFileName = Path.Combine(parentFolderInfo.FullName, $"{fileNameWithoutExtension}.old_{backupNumber}{oldFile.Extension}");
                    oldFile.MoveTo(newFileName);
                }
            }

            private void TryToDeleteTempFile()
            {
                try
                {
                    File.Delete(TempPath);
                }
                catch
                {
                    log4net.LogManager.GetLogger(GetType()).Error($"Could not delete temporary stored file '{TempPath}'");
                }
            }
        }
    }    
}