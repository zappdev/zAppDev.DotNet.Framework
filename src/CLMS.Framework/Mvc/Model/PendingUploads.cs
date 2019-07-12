using System;
using System.Collections.Generic;
using System.Linq;

#if NETFRAMEWORK
using System.Web;
#else
using CLMS.Framework.Utilities;
#endif

namespace CLMS.Framework.Mvc
{
    public partial class FileHelper
    {
        public class PendingUploads
        {
#if NETFRAMEWORK
            public List<UploadInfo> GetPendingFiles(string controller)
            {
                var key = $"{HttpContext.Current.Session.SessionID}{controller}_PendingFiles";
                return CLMS.AppDev.Cache.CacheManager.Current.Get(key, new List<UploadInfo>());
            }
#else
            public List<UploadInfo> GetPendingFiles(string controller)
            {
                var key = $"{controller}_PendingFiles";
                return Web.Session.Get(key, new List<UploadInfo>());
            }
#endif

#if NETFRAMEWORK
            public void UpdatePendingFiles(string controller, List<UploadInfo> value)
            {
                var key = $"{HttpContext.Current.Session.SessionID}{controller}_PendingFiles";
                CLMS.AppDev.Cache.CacheManager.Current.Set(key, value);
            }
#else
            public void UpdatePendingFiles(string controller, List<UploadInfo> value)
            {
                var key = $"{controller}_PendingFiles";
                Web.Session.Add(key, value);
            }
#endif
            
            public void Add(UploadInfo entry)
            {
                var currentPendingFiles = GetPendingFiles(entry.Controller);

                var existingEntry = currentPendingFiles.FirstOrDefault(f => f.Key == entry.Key);

                if (existingEntry != null)
                {
                    currentPendingFiles.Remove(existingEntry);
                }

                currentPendingFiles.Add(entry);

                UpdatePendingFiles(entry.Controller, currentPendingFiles);
            }

            public void Remove(UploadInfo entry)
            {
                var currentValue = GetPendingFiles(entry.Controller);

                currentValue.Remove(entry);

                UpdatePendingFiles(entry.Controller, currentValue);
            }

            public void RemoveByKey(string controller, string ownerId, int[] indexes = null)
            {
                var entries = GetPendingFiles(controller);
                var entry = Get(controller, ownerId, indexes);

                if (entry == null) return;

                entries.Remove(entry);

                //Correct the indexes of the collection to assure that keys are valid
                var relatedEntries = entries.Where(e => e.Key.StartsWith(ownerId)).ToList();
                for (int i = 0; i < relatedEntries.Count; i++)
                {
                    //Here we remove the last part of the parent indexes and reassign it
                    //in order to match the affected collection data-context-index values
                    //client-side
                    indexes = indexes ?? new int[] { };
                    var correctedIndexes = indexes.Take(indexes.Count() - 1).ToList();
                    correctedIndexes.Add(i);
                    entries[i].Indexes = correctedIndexes.ToArray();
                }

                UpdatePendingFiles(controller, entries);
            }

            public void Clear(string controller)
            {
                UpdatePendingFiles(controller, new List<UploadInfo>());
            }

            public void CommitAll(string controller, object viewModel, bool keepOldCopy = true)
            {
                _logger.Debug($"Commit all files called for controller [{controller}]");

                CommitAllFilesNew(controller, viewModel, keepOldCopy);
                CommitAllLegacy(controller, viewModel);
            }

            public void CommitAllFilesNew(string controller, object viewModel, bool keepOldCopy = true)
            {
                _logger.Debug($"Commit all files NEW called for controller [{controller}]");

                var entries = GetPendingFiles(controller).Where(e => !e.IsLegacy);
#if NETFRAMEWORK
                var username = HttpContext.Current?.Request?.GetOwinContext()?.Request?.User?.Identity?.Name;
#else
#endif

                foreach (var entry in entries)
                {
#if NETFRAMEWORK
                    _logger.Info($"File '{entry.Key}' was uploaded by '{username}'");
#else
                    _logger.Info($"File '{entry.Key}' was uploaded");
#endif
                    entry.Commit(viewModel, keepOldCopy);
                }

                while (entries.Any())
                {
                    Remove(entries.First());
                }
            }

            public void CommitByControl(string controller, object viewModel, string ownerId, bool keepOldCopy = true)
            {
                _logger.Debug($"Commit by control called for controller [{controller}] and control [{ownerId}]");

                var entries = GetByControl(controller, ownerId);

                foreach (var entry in entries)
                {
                    entry.Commit(viewModel, keepOldCopy);
                }

                while (entries.Any())
                {
                    Remove(entries.First());
                }
            }

            public UploadInfo Get(string controller, string ownerId, int[] indexes = null)
            {
                var entries = GetPendingFiles(controller);
                var requestedKey = ownerId + "_" + string.Join(";", indexes ?? new int[] { });

                return entries.FirstOrDefault(e => e.Key == requestedKey);
            }

            public UploadInfo GetFromOtherControls(string controller, string expextedRelativePath, List<string> otherControlIds, object viewModel, int[] indexes = null)
            {
                foreach (var control in otherControlIds)
                {
                    var entry = Get(controller, control, indexes);

                    if (entry == null) continue;

                    if (entry.GetRelativePath.Invoke(viewModel, indexes) == expextedRelativePath)
                    {
                        return entry;
                    }
                }

                return null;
            }

            public List<UploadInfo> GetByControl(string controller, string ownerId)
            {
                var entries = GetPendingFiles(controller);

                return entries.Where(e => e.OwnerControlId == ownerId).ToList();
            }

            [Obsolete("For Legacy Support")]
            public void CommitAllFileSystemWritten(string controller, object viewModel, bool keepOldCopy = true)
            {
                _logger.Debug($"CommitAllFileSystemWritten LEGACY called for controller [{controller}]");

                var entries = GetPendingFiles(controller).Where(e => e.IsWrittenToFileSystem && e.IsLegacy);

                foreach (var entry in entries)
                {
                    entry.Commit(viewModel, keepOldCopy);
                }

                while (entries.Any())
                {
                    Remove(entries.First());
                }
            }

            [Obsolete("For Legacy Support")]
            public void UpdateModelWithFilesBinaryData(string controller, object viewModel)
            {
                _logger.Debug($"UpdateModelWithFilesBinaryData LEGACY called for controller [{controller}]");

                var entries = GetPendingFiles(controller).Where(e => !e.IsWrittenToFileSystem && e.IsLegacy);

                foreach (var entry in entries)
                {
                    entry.Commit(viewModel);
                }

                while (entries.Any())
                {
                    Remove(entries.First());
                }
            }

            [Obsolete("For Legacy Support")]
            public void CommitAllLegacy(string controller, object viewModel)
            {
                _logger.Debug($"Commit all files LEGACY called for controller [{controller}]");

                CommitAllFileSystemWritten(controller, viewModel);
                UpdateModelWithFilesBinaryData(controller, viewModel);
            }

        }
    }    
}