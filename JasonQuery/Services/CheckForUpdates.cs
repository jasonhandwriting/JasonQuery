using JasonQuery.Core.Logging;
using JasonQuery.Core.Text;
using System;
using System.IO;
using System.Net;

namespace JasonQuery.Services
{
    internal delegate void BytesDownloadedEventHandler(ByteArgs e);

    public static class CheckForUpdates
    {
        /// <summary>Get update and version information from specified online file - returns a List</summary>
        /// <param name="downloadsUrl">URL to download file from</param>
        /// <param name="versionFile">Name of the pipe| delimited version file to download</param>
        /// <param name="resourceDownloadFolder">Folder on the local machine to download the version file to</param>
        /// <param name="targetFileName"></param>
        /// <returns>List containing the information from the pipe delimited version file</returns>
        public static string GetUpdateInfo(string downloadsUrl, string versionFile, string resourceDownloadFolder, string targetFileName)
        {
            //try downloading update info from the internet
            var updateCheck = WebData.DownloadFromWeb(downloadsUrl, versionFile, resourceDownloadFolder, targetFileName);

            //if the file is downloaded successfully
            if (!string.IsNullOrEmpty(updateCheck))
            {
                return string.Empty;
            }

            var versionText = File.ReadAllText($"{resourceDownloadFolder}{targetFileName}");
            var parts = versionText.Split(new[] { "```" }, StringSplitOptions.None);
            var versionProduct = parts.Length >= 1 ? parts[1] : string.Empty;
            var versionBeta = parts.Length >= 3 ? parts[2] : string.Empty; //20250410 第二個數字，存放測試版的號碼
            var version = versionProduct;

            if (!string.IsNullOrEmpty(versionBeta))
            {
                version = TextHelper.CompareVersions(versionBeta, versionProduct) == 1 ? versionBeta : versionProduct;
            }

            return version;
        }
    }

    internal class ByteArgs : EventArgs
    {
        public int Downloaded { get; set; }

        public int Total { get; set; }
    }

    internal static class WebData
    {
        public static event BytesDownloadedEventHandler BytesDownloaded;

        public static string DownloadFromWeb(string sURL, string sFileName, string sTargetFolder, string sTargetFileName)
        {
            try
            {
                var webReq = WebRequest.Create($"{sURL}{sFileName}");
                var webResponse = webReq.GetResponse();
                var dataStream = webResponse.GetResponseStream();

                //Download the data in chuncks
                var dataBuffer = new byte[1024];

                //Get the total size of the download
                var dataLength = (int)webResponse.ContentLength;

                //lets declare our downloaded bytes event args
                var byteArgs = new ByteArgs { Downloaded = 0, Total = dataLength };

                //we need to test for a null as if an event is not consumed we will get an exception
                BytesDownloaded?.Invoke(byteArgs);

                var memoryStream = new MemoryStream();

                while (true)
                {
                    if (dataStream == null)
                    {
                        continue;
                    }

                    var bytesFromStream = dataStream.Read(dataBuffer, 0, dataBuffer.Length);

                    if (bytesFromStream == 0)
                    {
                        byteArgs.Downloaded = dataLength;
                        byteArgs.Total = dataLength;
                        BytesDownloaded?.Invoke(byteArgs);

                        //Download complete
                        break;
                    }

                    //Write the downloaded data
                    memoryStream.Write(dataBuffer, 0, bytesFromStream);

                    byteArgs.Downloaded = bytesFromStream;
                    byteArgs.Total = dataLength;

                    BytesDownloaded?.Invoke(byteArgs);
                }

                //Convert the downloaded stream to a byte array
                var downloadedData = memoryStream.ToArray();

                dataStream.Close();
                memoryStream.Close();

                //寫入檔案
                var newFile = new FileStream($"{sTargetFolder}{sTargetFileName}", FileMode.Create);

                newFile.Write(downloadedData, 0, downloadedData.Length);
                newFile.Close();

                return string.Empty;
            }
            catch (Exception ex)
            {
                //網址錯誤或是網路不通？
                return TraceLogger.GetStackTraceMessageAndContent(ex.StackTrace, ex.Message);
            }
        }
    }
}