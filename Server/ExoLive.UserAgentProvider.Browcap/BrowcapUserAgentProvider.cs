using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using ExoLive.Server.Common.Models;
using ExoLive.Server.Common.Providers;
using Ionic.Zip;

namespace ExoLive.UserAgentProvider.Browcap
{
    public class BrowcapUserAgentProvider : UserAgentProviderBase
    {
        private class CsvLine : UserAgentInfo
        {
            public string Parent { get; set; }
        }

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
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var zipFile = ZipFile.Read(@"E:\Data\ExoLiveProject\Unsorted\browscap.zip"))
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
                string[] splitedLine;
                var csvLineList = new List<CsvLine>();
                var agentList = new List<UserAgentInfo>();
                using (var fs = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
                {
                    using (var sr = new StreamReader(fs))
                    {
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
                            splitedLine = line.Split(new[] { "\"," }, StringSplitOptions.RemoveEmptyEntries);
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

                            //var agent = new UserAgentInfo();
                            var csvLine = new CsvLine();
                            csvLine.BrowserVersionRelease = UserAgentInfo.BrowserVersionReleaseType.Release;
                            csvLine.ProcessorBits = UserAgentInfo.ProcessorBitsType.Bit32;
                            foreach (var column in splitedLine)
                            {
                                if (column == string.Empty) continue;

                                switch (dicColumn[colIndex])
                                {
                                    case "PropertyName":
                                        csvLine.UserAgent = GetStringWithoutQuotes(column);
                                        break;
                                    case "Browser":
                                        csvLine.BrowserName = GetStringWithoutQuotes(column);
                                        break;
                                    case "Version":
                                        csvLine.BrowserVersion = GetStringWithoutQuotes(column);
                                        break;
                                    case "MajorVer":
                                        csvLine.BrowserVersionMajor = GetStringWithoutQuotes(column);
                                        break;
                                    case "MinorVer":
                                        csvLine.BrowserVersionMinor = GetStringWithoutQuotes(column);
                                        break;
                                    case "Alpha":
                                        if (GetStringWithoutQuotes(column) == "true") csvLine.BrowserVersionRelease = UserAgentInfo.BrowserVersionReleaseType.Alpha;
                                        break;
                                    case "Beta":
                                        if (GetStringWithoutQuotes(column) == "true") csvLine.BrowserVersionRelease = UserAgentInfo.BrowserVersionReleaseType.Beta;
                                        break;
                                    case "Crawler":
                                        csvLine.IsCrawler = GetStringWithoutQuotes(column) == "true";
                                        break;
                                    case "isMobileDevice":
                                        csvLine.IsMobileDevice = GetStringWithoutQuotes(column) == "true";
                                        break;
                                    case "isSyndicationReader":
                                        csvLine.IsSyndicationReader = GetStringWithoutQuotes(column) == "true";
                                        break;
                                    case "isTablet":
                                        csvLine.IsTablet = GetStringWithoutQuotes(column) == "true";
                                        break;
                                    case "Parent":
                                        csvLine.Parent = GetStringWithoutQuotes(column);
                                        break;
                                    case "Platform":
                                        csvLine.PlatformName = GetStringWithoutQuotes(column);
                                        break;
                                    case "Platform_Version":
                                        csvLine.PlatformVersion = GetStringWithoutQuotes(column);
                                        break;
                                    case "Win16":
                                        if (GetStringWithoutQuotes(column) == "true") csvLine.ProcessorBits = UserAgentInfo.ProcessorBitsType.Bit16;
                                        break;
                                    case "Win32":
                                        if (GetStringWithoutQuotes(column) == "true") csvLine.ProcessorBits = UserAgentInfo.ProcessorBitsType.Bit32;
                                        break;
                                    case "Win64":
                                        if (GetStringWithoutQuotes(column) == "true") csvLine.ProcessorBits = UserAgentInfo.ProcessorBitsType.Bit64;
                                        break;
                                }

                                colIndex++;
                            }
                            csvLineList.Add(csvLine);
                            index++;
                            Debug.WriteLine(index);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                isError = true;
            }
            finally
            {
                if (!isError)
                {
                    var lastSuccessUpdateDateTimeString = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                    _dataProvider.SetOption("ExoLive.UserAgentProvider.Browcap.LastSuccessUpdateDateTime", lastSuccessUpdateDateTimeString);
                }
                if (File.Exists(tempFile)) File.Delete(tempFile);
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
            throw new NotImplementedException();
        }
    }
}
