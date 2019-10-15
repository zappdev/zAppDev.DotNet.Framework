// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else

using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using zAppDev.DotNet.Framework.Locales;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Mvc.Application
{
    public class ApplicationInfoService : IApplicationInfoService
    {
        private static string _appVersion;

        private static long _lastOnChange;
        private ILogger<ApplicationInfoService> _logger;
        private readonly ILocalesService _localesService;
        private static readonly object SyncRoot = new Object();

        public ApplicationInfoService(ILogger<ApplicationInfoService> logger,
            ILocalesService localesService)
        {
            _logger = logger;
            _localesService = localesService;
        }

        public string AppVersion
        {
            get
            {
                if (string.IsNullOrEmpty(_appVersion))
                {
                    lock (SyncRoot)
                    {
                        if (string.IsNullOrEmpty(_appVersion))
                        {
                            var dir = Web.MapPath("");
                            var path = Path.Combine(dir, "projectInfo.json");
                            using (var watcher = new FileSystemWatcher
                            {
                                Path = dir,
                                NotifyFilter = NotifyFilters.LastWrite,
                                Filter = "*.json",
                                IncludeSubdirectories = false
                            })
                            {
                                watcher.Changed += new FileSystemEventHandler(OnChanged);
                                watcher.EnableRaisingEvents = true;
                            }
                            UpdateAppVersion(path);
                        }
                    }
                }
                return _appVersion;
            }
        }

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            var diff = (DateTime.Now.Ticks - _lastOnChange) / TimeSpan.TicksPerMillisecond;
            if (diff < 500)
            {
                return;
            }
            _lastOnChange = DateTime.Now.Ticks;
            _logger.LogInformation("Project Info Changed, updating app version");
            UpdateAppVersion(e.FullPath);
        }

        private void UpdateAppVersion(string path, int @try = 0)
        {
            try
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var pi = Newtonsoft.Json.JsonConvert.DeserializeObject<ProjectInfo>(reader.ReadToEnd());
                        _appVersion = pi.Version ?? Guid.NewGuid().ToString();
                        ClearLocalesCache();
                    }
                }
            }
            catch (Exception e)
            {
                if (@try < 5)
                {
                    Thread.Sleep(100);
                    _logger.LogInformation($"Failed to read Project Info, retry... ({@try + 1})");
                    UpdateAppVersion(path, ++@try);
                    return;
                }
                _appVersion = Guid.NewGuid().ToString();
                _logger.LogError($"Failed to get app version from path {path}", e);
            }
        }

        private void ClearLocalesCache()
        {
            _localesService.ClearLocalesCache();
        }

    }
}

#endif
