using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace CodeEndeavors.ServiceHost
{
    public class Update
    {

        public static string ResolvePath(string path)
        {
            return path.StartsWith("~/") ? HostingEnvironment.MapPath(path) : path;
        }

        public static string UpdateDir
        {
            get
            {
                var dir = ResolvePath("~/_updates/");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                return dir;
            }
        }
        public static string PackageDir
        {
            get
            {
                var dir = ResolvePath("~/_packages/");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                return dir;
            }
        }

        public static void WatchForUpdates()
        {
            Common.Services.Logging.Debug("Watching folder {0} for updates", UpdateDir);
            ApplyUpdates();
            //using cache dependency to easily monitor update folder for changes
            HttpRuntime.Cache.Add("_updates", "", new CacheDependency(UpdateDir), Cache.NoAbsoluteExpiration, TimeSpan.FromHours(1), CacheItemPriority.NotRemovable, new CacheItemRemovedCallback(OnFolderChanged));
        }

        private static void OnFolderChanged(string key, object value, CacheItemRemovedReason reason)
        {
            Common.Services.Logging.Info("Detected new file in update folder: {0} - {1} - {2}", UpdateDir, reason, value);
            HandleFolderChanged(false);
        }

        private static void HandleFolderChanged(bool afterErrorTry)
        {
            HttpRuntime.Cache.Add("_updates", "", new CacheDependency(UpdateDir), Cache.NoAbsoluteExpiration, TimeSpan.FromHours(1), CacheItemPriority.NotRemovable, new CacheItemRemovedCallback(OnFolderChanged));
            try
            {
                ApplyUpdates();
            }
            catch (Exception ex)
            {
                Common.Services.Logging.Error("Error applying update {0}", ex.Message);
                if (!afterErrorTry)
                {
                    System.Threading.Thread.Sleep(500); //try one more time
                    HandleFolderChanged(true);
                }
            }
        }

        public static int ApplyUpdates()
        {
            var dir = new DirectoryInfo(UpdateDir);
            var count = 0;
            var files = dir.GetFiles("*.zip");
            foreach (var file in files)
                count += InstallFile(file.FullName) ? 1 : 0;
            return count;
        }

        public static bool InstallFile(string fileName, string portalId = null)
        {
            var extractDir = ResolvePath("~/");
            var file = new FileInfo(fileName);
            switch (file.Extension.ToLower())
            {
                case ".zip":
                    {
                        var packageFileName = Path.Combine(PackageDir, new FileInfo(fileName).Name);
                        if (File.Exists(packageFileName))
                            File.Delete(packageFileName);
                        File.Move(fileName, packageFileName);

                        //if dll in root, then we need to extract directly into bin folder
                        var rootContainsDll = GetZipFileList(packageFileName, e => e.IndexOf("/") == -1 && e.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase)).Count > 0;
                        if (rootContainsDll)
                            extractDir = Path.Combine(extractDir, "bin");

                        Common.Services.Logging.Info("Applying update for file: {0}", packageFileName);
                        ExtractZip(packageFileName, extractDir, @"-package\.manifest;-\.json;-\.config");   //don't overwrite config file if exists
                        ExtractZip(packageFileName, extractDir, @"\.config", overwrite: false); //if it doesn't exist, add config file

                        return true;
                    }
                default:
                    {
                        Common.Services.Logging.Error("Unknown File Extension: " + file.Extension);
                        break;
                    }
            }
            return false;
        }

        public static void ExtractZip(string fileName, string targetDirectory, string fileFilter = null, string directoryFilter = null, bool overwrite = true)
        {
            var zip = new FastZip();
            zip.ExtractZip(fileName, targetDirectory, overwrite ? FastZip.Overwrite.Always : FastZip.Overwrite.Never, null, fileFilter, directoryFilter, true);
        }

        public static List<string> GetZipFileList(string fileName, Func<string, bool> where = null)
        {
            var ret = new List<string>();
            using (var fs = File.OpenRead(fileName))
            {
                using (var zip = new ZipFile(fs))
                {
                    foreach (ZipEntry entry in zip)
                    {
                        //if (entry.IsFile)
                        ret.Add(entry.Name);
                    }
                }
            }
            if (where != null)
                ret = ret.Where(where).ToList();
            return ret;
        }

    }
}