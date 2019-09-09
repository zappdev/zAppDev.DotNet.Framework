// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
using zAppDev.DotNet.Framework.Mvc;
using log4net;
using System.Collections.Generic;
using System.IO;
using System.Web.Optimization;

namespace zAppDev.DotNet.Framework.Mvc
{

    internal class VersionBundleTransform : IBundleTransform
    {
        private readonly string _applicationVersion; 

        public VersionBundleTransform(string applicationVersion)
        {
            _applicationVersion = applicationVersion; 
        }

        public void Process(BundleContext context, BundleResponse response)
        {
            string contentType = null;

            foreach (var file in response.Files)
            {
                if (string.IsNullOrWhiteSpace(contentType))
                {
                    if (Path.GetExtension(file.IncludedVirtualPath) == ".js")
                    {
                        contentType = "text/javascript";
                    }
                    else if (Path.GetExtension(file.IncludedVirtualPath) == ".css")
                    {
                        contentType = "text/css";
                    }
                }

                file.IncludedVirtualPath = $"{file.IncludedVirtualPath}?v={_applicationVersion}";
                file.Transforms.Add(new CssRewriteUrlTransform());
            }
            response.ContentType = contentType;
        }
    }//end VersionBundleTransform()

    internal class CustomBundleOrderer : IBundleOrderer
    {
        public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> files)
        {
            return files;
        }
    }

    internal static class CustomBundleTransformer
    {
        public static System.Web.Optimization.Bundle WithCustomTransformation(this System.Web.Optimization.Bundle bundle, string applicationVersion, bool addOrderer)
        {
            bundle.Transforms.Clear();
            bundle.Transforms.Add(new VersionBundleTransform(applicationVersion));
            if (addOrderer)
            {
                bundle.Orderer = new CustomBundleOrderer();
            }
            return bundle;
        }
    }

    public class Bundles
    {
        public List<Bundle> Scripts { get; set; }
        public List<Bundle> Styles { get; set; }
        public Bundles()
        {
            Scripts = new List<Bundle>();
            Styles = new List<Bundle>();
        }
    }

    public class Bundle
    {
        public string Name { get; set; }

        public List<string> Paths { get; set; }

        public Bundle()
        {
            Paths = new List<string>();
        }
    }

    public class BundleHelper
    {
        private readonly string _applicationVersion; 

        private readonly string _bundlesFile = "bundlesInfo.json";

        private readonly ILog _logger;

        public string DefaultTagFormatForStyles => $"<link href='{{0}}?v={_applicationVersion}' rel='stylesheet'/>";

        public string DefaultTagFormatForScripts => $"<script src='{{0}}?v={_applicationVersion}'></script>";
    
        public BundleHelper()
        {
            _logger = LogManager.GetLogger(this.GetType());
            _applicationVersion = BaseViewPageBase<object>.AppVersion;
        }

        public void AddFromJson(BundleCollection bundles)
        {
            try
            {
                var root = System.Web.Hosting.HostingEnvironment.MapPath("~");
                var bundlesInfoFile = Path.Combine(root, _bundlesFile);

                if (!File.Exists(bundlesInfoFile))
                {
                    _logger.Warn($"Cannot register the Application's Bundles. File not found: {bundlesInfoFile}. Ignore this Warning if you do not have any Web Pages in your Application.");
                    return;
                }

                var bundlesInfoContent = File.ReadAllText(bundlesInfoFile);
                if (string.IsNullOrWhiteSpace(bundlesInfoContent))
                {
                    _logger.Warn($"Cannot register the Application's Bundles. Found and empty file: {bundlesInfoFile}. Ignore this Warning if you do not have any Web Pages in your Application.");
                    return;
                }

                var jsonBundles = Newtonsoft.Json.JsonConvert.DeserializeObject<Bundles>(bundlesInfoContent);
                if(jsonBundles == null)
                {
                    _logger.Warn($"Deserialization of the JSON Bundles returned NULL. Ignore this Warning if you do not have any Web Pages in your Application.");
                    return;
                }

                if((jsonBundles.Scripts == null || jsonBundles.Scripts.Count == 0) && (jsonBundles.Styles == null || jsonBundles.Styles.Count == 0))
                {
                    _logger.Warn($"Found no Styles or Scripts in: {bundlesInfoFile}. Ignore this Warning if you do not have any Web Pages in your Application.");
                    return;
                }

                foreach (var script in jsonBundles.Scripts)
                {
                    if (script.Paths.Count == 0)
                    {
                        continue;
                    }

                    var bundle = new ScriptBundle(script.Name);
                    foreach (var path in script.Paths)
                    {
                        bundle.Include(path);
                    }
                    bundles.Add(bundle.WithCustomTransformation(_applicationVersion, true));
                }

                foreach(var style in jsonBundles.Styles)
                {
                    if(style.Paths.Count == 0)
                    {
                        continue;
                    }

                    var bundle = new StyleBundle(style.Name);
                    foreach(var path in style.Paths)
                    {
                        bundle.Include(path);
                    }

                    bundles.Add(bundle.WithCustomTransformation(_applicationVersion, false));
                }

            }
            catch (System.Exception ex)
            {
                _logger.Error($"Caught a [{ex.GetType()}] Exception while Registering Bundles: {ex.Message}");
                _logger.Error(ex.StackTrace);
                throw;
            }
        }//end AddFromJson()
    }//end class BundleHelper
}//end namespace

#endif