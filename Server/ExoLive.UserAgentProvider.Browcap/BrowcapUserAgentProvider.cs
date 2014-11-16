using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Net;
using ExoLive.Server.Common.Models;
using ExoLive.Server.Common.Providers;
using Ionic.Zip;

namespace ExoLive.UserAgentProvider.Browcap
{
    public class BrowcapUserAgentProvider : UserAgentProviderBase
    {
        private DataProviderBase _dataProvider;
        private const string ConfigUpdateIntervalMinutes = "updateIntervalMinutes";
        private NameValueCollection _config;

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
            _config = config;
        }

        private int InternalUpdateIntervalMinutes
        {
            get
            {
                var val = _config[ConfigUpdateIntervalMinutes];
                return Convert.ToInt32(val);
            }
        }

        public override int CheckTest(int source)
        {
            return source;
        }

        public override void SetDataProvider(DataProviderBase dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public override bool IsNeedDataUpdate()
        {
            var lastSuccessUpdateDateTimeString =
                _dataProvider.GetOption("ExoLive.UserAgentProvider.Browcap.LastSuccessUpdateDateTime",
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm"));

            var parseVal = lastSuccessUpdateDateTimeString.Value.Split(' ');
            if (parseVal.Length == 2)
            {
                var parseDate = parseVal[0].Split('.');
                var parseTime = parseVal[1].Split(':');

                if (parseDate.Length == 3 && parseTime.Length == 2)
                {
                    var lastSuccessUpdateDateTime = new DateTime(Convert.ToInt32(parseDate[2]),
                        Convert.ToInt32(parseDate[1]),
                        Convert.ToInt32(parseDate[0]),
                        Convert.ToInt32(parseTime[0]),
                        Convert.ToInt32(parseTime[1]), 0);

                    var timeAgo = DateTime.Now - lastSuccessUpdateDateTime;

                    return timeAgo.TotalMinutes > InternalUpdateIntervalMinutes;
                }
            }

            return false;
        }

        public override void DataUpdate()
        {
            bool isError = false;
            bool isSuccess = false;
            var tempZip = Path.GetTempFileName();
            var tempFile = Path.GetTempFileName();
            IDbConnection cnn = null;
            IDbTransaction txn = null;
            try
            {
                using (var web = new WebClient())
                {
                    web.DownloadFile("http://browscap.org/stream?q=BrowsCapZIP", tempZip);
                }
                using (var zipFile = ZipFile.Read(tempZip))
                {
                    foreach (ZipEntry entry in zipFile)
                    {
                        if (entry.Source == ZipEntrySource.ZipFile && string.Compare(entry.FileName, "browscap.csv", StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            using (var fs = new FileStream(tempFile, FileMode.Open, FileAccess.Write))
                            {
                                entry.Extract(fs);
                            }
                        }
                    }
                }

                var dicColumn = new Dictionary<int, string>();
                int index = 0;
                //var csvLineList = new List<CsvLine>();
                using (var fs = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        cnn = _dataProvider.CreateConnection();
                        txn = cnn.BeginTransaction();
                        _dataProvider.DeleteAllUserAgentInfo(txn);
                        while (true)
                        {
                            var line = sr.ReadLine();
                            if (line == null) break;
                            if (index < 2)
                            {
                                index++;
                                continue;
                            }

                            int colIndex = 0;
                            string[] splitedLine = line.Split(new[] { "\"," }, StringSplitOptions.RemoveEmptyEntries);
                            if (index == 2)
                            {
                                foreach (var column in splitedLine)
                                {
                                    if (column == string.Empty) continue;
                                    dicColumn.Add(colIndex, GetStringWithoutQuotes(column));
                                    colIndex++;
                                }
                                index++;
                                continue;
                            }

                            var userAgentInfo = new UserAgentInfo
                            {
                                BrowserVersionRelease = UserAgentInfo.BrowserVersionReleaseType.Release,
                                ProcessorBits = UserAgentInfo.ProcessorBitsType.Bit32
                            };
                            foreach (var column in splitedLine)
                            {
                                if (column == string.Empty) continue;

                                switch (dicColumn[colIndex])
                                {
                                    case "PropertyName":
                                        userAgentInfo.UserAgent = GetStringWithoutQuotes(column);
                                        break;
                                    case "Browser":
                                        userAgentInfo.BrowserName = GetStringWithoutQuotes(column);
                                        break;
                                    case "Version":
                                        userAgentInfo.BrowserVersion = GetStringWithoutQuotes(column);
                                        break;
                                    case "MajorVer":
                                        userAgentInfo.BrowserVersionMajor = GetStringWithoutQuotes(column);
                                        break;
                                    case "MinorVer":
                                        userAgentInfo.BrowserVersionMinor = GetStringWithoutQuotes(column);
                                        break;
                                    case "Alpha":
                                        if (GetStringWithoutQuotes(column) == "true") userAgentInfo.BrowserVersionRelease = UserAgentInfo.BrowserVersionReleaseType.Alpha;
                                        break;
                                    case "Beta":
                                        if (GetStringWithoutQuotes(column) == "true") userAgentInfo.BrowserVersionRelease = UserAgentInfo.BrowserVersionReleaseType.Beta;
                                        break;
                                    case "Crawler":
                                        userAgentInfo.IsCrawler = GetStringWithoutQuotes(column) == "true";
                                        break;
                                    case "isMobileDevice":
                                        userAgentInfo.IsMobileDevice = GetStringWithoutQuotes(column) == "true";
                                        break;
                                    case "isSyndicationReader":
                                        userAgentInfo.IsSyndicationReader = GetStringWithoutQuotes(column) == "true";
                                        break;
                                    case "isTablet":
                                        userAgentInfo.IsTablet = GetStringWithoutQuotes(column) == "true";
                                        break;
                                    //case "Parent":
                                    //    userAgentInfo.Parent = GetStringWithoutQuotes(column);
                                    //    break;
                                    case "Platform":
                                        userAgentInfo.PlatformName = GetStringWithoutQuotes(column);
                                        break;
                                    case "Platform_Version":
                                        userAgentInfo.PlatformVersion = GetStringWithoutQuotes(column);
                                        break;
                                    case "Win16":
                                        if (GetStringWithoutQuotes(column) == "true") userAgentInfo.ProcessorBits = UserAgentInfo.ProcessorBitsType.Bit16;
                                        break;
                                    case "Win32":
                                        if (GetStringWithoutQuotes(column) == "true") userAgentInfo.ProcessorBits = UserAgentInfo.ProcessorBitsType.Bit32;
                                        break;
                                    case "Win64":
                                        if (GetStringWithoutQuotes(column) == "true") userAgentInfo.ProcessorBits = UserAgentInfo.ProcessorBitsType.Bit64;
                                        break;
                                }

                                colIndex++;
                            }

                            _dataProvider.InsertUserAgentInfoBulk(new List<UserAgentInfo> { userAgentInfo }, txn);
                            index++;
                        }
                        txn.Commit();
                    }
                }
                isSuccess = true;
            }
            catch
            {
                isError = true;
                if (txn != null)
                {
                    txn.Rollback();
                    txn.Dispose();
                }
                if (cnn != null)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
            finally
            {
                if (!isError && isSuccess)
                {
                    var lastSuccessUpdateDateTimeString = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                    _dataProvider.SetOption("ExoLive.UserAgentProvider.Browcap.LastSuccessUpdateDateTime", lastSuccessUpdateDateTimeString);
                }
                if (File.Exists(tempFile)) File.Delete(tempFile);
                if (File.Exists(tempZip)) File.Delete(tempZip);
            }
        }

        private string GetStringWithoutQuotes(string source)
        {
            var str = source.Trim();
            bool start = str.StartsWith("\"");
            bool stop = str.EndsWith("\"");

            if (start && stop && str.Length >= 2) return str.Substring(1, str.Length - 2);
            if (start && str.Length >= 1) return str.Substring(1, str.Length - 1);
            if (stop && str.Length >= 1) return str.Substring(0, str.Length - 1);

            return str;
        }

        public override UserAgentInfo GetUserAgentInfo(string userAgent)
        {
            return _dataProvider.GetUserAgentInfo(userAgent);
        }
    }
}
