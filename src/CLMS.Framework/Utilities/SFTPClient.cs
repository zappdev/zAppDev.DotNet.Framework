using log4net;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLMS.Framework.Utilities
{
    public class SFTPClient
    {
        public string Host { get; set; }
        private int _port;
        public int Port
        {
            get
            {
                return _port > 0 ? _port : 22;
            }
            set
            {
                _port = value;
            }
        }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool ThrowErrors { get; set; }
        private ILog logger;

        public SFTPClient()
        {
            logger = LogManager.GetLogger(this.GetType());
        }

        public List<string> ListDirectory(string remoteDirectory)
        {
            return SearchDirectory(remoteDirectory, "", true, false);
        }

        public List<string> SearchDirectory(string remoteDirectory, string pattern, bool includeDirectories, bool recursive)
        {
            List<string> results;

            //Create a client
            using (var sftp = new SftpClient(Host, Port, Username, Password))
            {
                //Connect
                sftp.Connect();
                //...and search for whaterver you want
                results = SearchDirectory(remoteDirectory, pattern, includeDirectories, recursive, sftp);
                //Finally disconnect and dispose
                sftp.Disconnect();
            }

            return results;
        }

        private List<string> SearchDirectory(string remoteDirectory, string pattern, bool includeDirectories, bool recursive, SftpClient sftp)
        {
            var results = new List<string>();
            //Basic checks
            if (!sftp.IsConnected) logger.Error("SFTP client is not connected!");
            if (string.IsNullOrWhiteSpace(remoteDirectory)) remoteDirectory = ".";
            //Get listing
            var files = sftp.ListDirectory(remoteDirectory);
            foreach (var file in files)
            {
                //Should not mess with current and parrent directories
                if (file.FullName.EndsWith(".") || file.FullName.EndsWith("..")) continue;
                //Add match when needed
                if ((string.IsNullOrEmpty(pattern) || file.FullName.Contains(pattern)) && ((file.IsDirectory && includeDirectories) || !file.IsDirectory))
                    results.Add(file.FullName);

                //Check for recursive search
                if (recursive && file.IsDirectory)
                {
                    //Recursive call
                    var response = SearchDirectory(file.FullName, pattern, includeDirectories, recursive, sftp);
                    //Add results
                    if (response != null && response.Count > 0)
                        results.AddRange(response);
                }
            }
            return results;
        }

        public bool DownloadFiles(List<string> remoteFilePaths, string localFolderPath)
        {
            var errorOccured =false;

            //Create a client
            using (var sftp = new SftpClient(Host, Port, Username, Password))
            {
                //Connect
                sftp.Connect();
                //Iterate the collection
                foreach (var remoteFilepath in remoteFilePaths)
                {
                    //Get the proper local folder paths
                    var remoteFilename = remoteFilepath.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                    if (string.IsNullOrWhiteSpace(remoteFilename))
                    {
                        logger.Error($"Specified remote file {remoteFilepath} is invalid");
                        continue;
                    }
                    try
                    {
                        //...and download each file
                        using (var file = File.OpenWrite(Path.Combine(localFolderPath, remoteFilename)))
                        {
                            sftp.DownloadFile(remoteFilepath, file);
                        }
                    }
                    catch (Exception ex)
                    {
                        errorOccured = true;
                        logger.Error(ex.Message, ex);
                        if (ThrowErrors)
                            throw new Exception($"CLMS.Framework SFTPClient error in downloading file: {remoteFilepath}");
                    }
                }
                //Finally disconnect and dispose
                sftp.Disconnect();
            }

            return !errorOccured;
        }

        public bool UploadFiles(List<string> localFilePaths, string remoteFolder)
        {
            var errorOccured = false;

            //Create a client
            using (var sftp = new SftpClient(Host, Port, Username, Password))
            {
                //Connect
                sftp.Connect();

                sftp.BufferSize = 4 * 1024;
                //Iterate the collection
                foreach (var localFilePath in localFilePaths)
                {
                    //Prepare vars
                    var fileInfo = new FileInfo(localFilePath);
                    var remoteFilePath = remoteFolder.EndsWith("/") ? $"{remoteFolder}{fileInfo.Name}" : $"{remoteFolder}/{fileInfo.Name}";

                    try
                    {
                        //Open a filestream
                        using (var fileStream = new FileStream(localFilePath, FileMode.Open))
                        {
                            //...and upload the file
                            sftp.UploadFile(fileStream, remoteFilePath, null);
                        }
                    }
                    catch (Exception ex)
                    {
                        errorOccured = true;
                        logger.Error(ex.Message, ex);
                        if (ThrowErrors)
                            throw new Exception($"CLMS.Framework SFTPClient error in uploading file: {fileInfo.Name}");
                    }
                }
                //Finally disconnect and dispose
                sftp.Disconnect();
            }

            return !errorOccured;
        }

        public bool DeleteFilesAndFolders(List<string> remotePaths)
        {
            var errorOccured = false;

            //Create a client
            using (var sftp = new SftpClient(Host, Port, Username, Password))
            {
                //Connect
                sftp.Connect();
                //Iterate the collection
                foreach (var remoteFilepath in remotePaths)
                {
                    try
                    {
                        //...and delete each file
                        sftp.Delete(remoteFilepath);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.Message, ex);
                        if (ThrowErrors)
                            throw new Exception($"CLMS.Framework SFTPClient error in deleting file: {remoteFilepath}");
                    }
                }
                //Finally disconnect and dispose
                sftp.Disconnect();
            }

            return !errorOccured;
        }
    }
}
