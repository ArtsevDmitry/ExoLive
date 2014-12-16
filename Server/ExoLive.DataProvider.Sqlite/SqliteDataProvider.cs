using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ExoLive.Server.Common.Internationalization;
using ExoLive.Server.Common.Models;
using ExoLive.Server.Common.Providers;

namespace ExoLive.DataProvider.SqLite
{
    /// <summary>
    /// DataProvider implementation for SqLite database engine
    /// </summary>

    public class SqliteDataProvider : DataProviderBase
    {
        private const string ConfigParamDatabaseFileName = "databaseFileName";
        private NameValueCollection _config;

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
            _config = config;
        }

        private string InternalDatabaseFileName
        {
            get { return _config[ConfigParamDatabaseFileName]; }
        }

        private string InternalConnectionString
        {
            get { return string.Format("data source={0}", InternalDatabaseFileName); }
        }

        private SQLiteConnection CreateConnection(bool foreygnKeyPolicy = true)
        {
            var cnn = new SQLiteConnection(InternalConnectionString);
            cnn.Open();
            if (foreygnKeyPolicy)
            {
                //Enable foreygn key policy on new connection
                ExecuteServiceCommand(cnn, "PRAGMA foreign_keys = ON;");
            }
            return cnn;
        }

        private SQLiteCommand CreateCommand(SQLiteConnection cnn)
        {
            return new SQLiteCommand(cnn);
        }

        /// <summary>
        /// Execute service command and not close connection
        /// </summary>
        /// <param name="cnn">Connection object</param>
        /// <param name="commandText">Sql command</param>
        private void ExecuteServiceCommand(SQLiteConnection cnn, string commandText)
        {
            using (var cmd = CreateCommand(cnn))
            {
                cmd.CommandText = commandText;
                cmd.ExecuteNonQuery();
            }
        }

        private SQLiteCommand CreateCommand(SQLiteTransaction txn)
        {
            var cmd = new SQLiteCommand
            {
                Connection = txn.Connection,
                Transaction = txn
            };
            return cmd;
        }

        private SQLiteCommand CreateCommand(object dataStateObj)
        {
            var txn = dataStateObj as SQLiteTransaction;
            var cnn = txn != null ? txn.Connection : dataStateObj as SQLiteConnection;

            var cmd = new SQLiteCommand { Connection = cnn, Transaction = txn };

            return cmd;
        }

        public override IDbConnection CreateConnection()
        {
            return CreateConnection();
        }

        public override int CheckTest(int source)
        {
            return source;
        }

        public override Option SetOption(string key, string value, object objCnnOrTxn = null)
        {
            var optionResult = new Option
            {
                Id = Guid.NewGuid().ToString(),
                Key = key,
                Value = value
            };

            using (var mgr = new DataContextManager<Option>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    var existentOption = GetOption(key, "OPTION_NOT_FOUND", state.DataStateObj);
                    if (existentOption.Value == "OPTION_NOT_FOUND")
                    {
                        using (var cmd = CreateCommand(state.DataStateObj))
                        {
                            const string sql = @"INSERT INTO OptionTable(Id, ""Key"", Value) VALUES(@Id, @Key, @Value);";
                            cmd.CommandText = sql;
                            cmd.Parameters.AddWithValue("Id", optionResult.Id);
                            cmd.Parameters.AddWithValue("Key", optionResult.Key);
                            cmd.Parameters.AddWithValue("Value", optionResult.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var cmd = CreateCommand(state.DataStateObj))
                        {
                            const string sql = @"UPDATE OptionTable SET Value=@Value WHERE Id=@Id;";
                            cmd.CommandText = sql;
                            cmd.Parameters.AddWithValue("Id", optionResult.Id);
                            cmd.Parameters.AddWithValue("Key", optionResult.Key);
                            cmd.Parameters.AddWithValue("Value", optionResult.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    state.SafeCommit();

                    state.Result = optionResult;
                });

                return mgr.Result;
            }
        }

        public override Option GetOption(string key, string defaultValue = null, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<Option>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        const string sql = @"SELECT ""Id"", ""Key"", ""Value"" FROM OptionTable WHERE Key='@Key';";
                        cmd.CommandText = sql;
                        cmd.Parameters.AddWithValue("Key", key);
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                                state.Result = new Option
                                {
                                    Id = rd.ToNullString("Id", string.Empty),
                                    Key = key,
                                    Value = rd.ToNullString("Value", defaultValue)
                                };
                        }
                    }
                });

                return mgr.Result ?? new Option
                {
                    Id = string.Empty,
                    Key = key,
                    Value = defaultValue
                };
            }
        }

        public override List<Option> GetOptions(string keyData, OptionSearchPattern searchPattern, string defaultValue = null, object objCnnOrTxn = null)
        {
            string sqlPattern;
            switch (searchPattern)
            {
                case OptionSearchPattern.Equals:
                    sqlPattern = "Key='@Key'";
                    break;
                case OptionSearchPattern.Begin:
                    sqlPattern = "Key LIKE '@Key%'";
                    break;
                case OptionSearchPattern.End:
                    sqlPattern = "Key LIKE '%@Key'";
                    break;
                case OptionSearchPattern.Contain:
                    sqlPattern = "Key LIKE '%@Key%'";
                    break;
                default:
                    sqlPattern = "Key='@Key'";
                    break;
            }


            using (var mgr = new DataContextManager<List<Option>>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    var sql = string.Format(@"SELECT ""Id"", ""Key"", ""Value"" FROM OptionTable WHERE {0};", sqlPattern);
                    var results = new List<Option>();

                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = sql;
                        cmd.Parameters.AddWithValue("Key", keyData);
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                                results.Add(new Option
                                {
                                    Id = rd.ToNullString("Id", string.Empty),
                                    Key = rd.ToNullString("Key", string.Empty),
                                    Value = rd.ToNullString("Value", defaultValue)
                                });
                        }
                    }

                    state.Result = results;
                });

                return mgr.Result;
            }
        }

        public override ApiKeyInfo GetApiKeyInfo(string apiKey, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<ApiKeyInfo>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    const string sql = @"SELECT A.Id, A.CompanyId, A.Key, A.Description, A.IsActive FROM ApiKeyTable A WHERE A.Key=@Key;
SELECT D.Id, D.WebSiteName, D.Domain, D.CompanyId 
FROM ApiKeyTable A 
INNER JOIN
ApiKeyRefWebDomainTable Ref ON Ref.ApiKeyId=A.Id
INNER JOIN 
WebDomainTable D ON D.Id=Ref.WebDomainId
WHERE A.Key=@Key;";

                    var apiKeyInfo = new ApiKeyInfo();

                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = sql;
                        cmd.Parameters.AddWithValue("Key", apiKey);

                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                apiKeyInfo.Id = rd.ToNullString("Id", string.Empty);
                                apiKeyInfo.CompanyId = rd.ToNullString("CompanyId", string.Empty);
                                apiKeyInfo.Key = rd.ToNullString("Key", string.Empty);
                                apiKeyInfo.Description = rd.ToNullString("Description", string.Empty);
                                apiKeyInfo.IsActive = rd.ToNullBool("IsActive", false);
                            }

                            if (rd.NextResult())
                            {
                                while (rd.Read())
                                {
                                    var webDomain = new WebDomain
                                    {
                                        Id = rd.ToNullString("Id", string.Empty),
                                        WebSiteName = rd.ToNullString("WebSiteName", string.Empty),
                                        Domain = rd.ToNullString("Domain", string.Empty),
                                        CompanyId = rd.ToNullString("CompanyId", string.Empty)
                                    };
                                    apiKeyInfo.WebDomains.Add(webDomain);
                                }
                            }
                        }
                    }

                    state.Result = apiKeyInfo;
                });

                return mgr.Result;
            }
        }

        public override void InsertUserAgentInfoBulk(List<UserAgentInfo> items, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<object>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = "INSERT INTO UserAgentInfoTable (Id, UserAgent, BrowserName, BrowserVersion, BrowserVersionMajor, BrowserVersionMinor, BrowserVersionRelease, PlatformName, PlatformVersion, ProcessorBits, IsMobileDevice, IsTablet, IsSyndicationReader, IsCrawler) VALUES (@Id, @UserAgent, @BrowserName, @BrowserVersion, @BrowserVersionMajor, @BrowserVersionMinor, @BrowserVersionRelease, @PlatformName, @PlatformVersion, @ProcessorBits, @IsMobileDevice, @IsTablet, @IsSyndicationReader, @IsCrawler);";

                        foreach (var userAgentInfo in items)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("Id", Guid.NewGuid().ToString());
                            cmd.Parameters.AddWithValue("UserAgent", userAgentInfo.UserAgent);
                            cmd.Parameters.AddWithValue("BrowserName", userAgentInfo.BrowserName);
                            cmd.Parameters.AddWithValue("BrowserVersion", userAgentInfo.BrowserVersion);
                            cmd.Parameters.AddWithValue("BrowserVersionMajor", userAgentInfo.BrowserVersionMajor);
                            cmd.Parameters.AddWithValue("BrowserVersionMinor", userAgentInfo.BrowserVersionMinor);
                            cmd.Parameters.AddWithValue("BrowserVersionRelease", userAgentInfo.BrowserVersionRelease);
                            cmd.Parameters.AddWithValue("PlatformName", userAgentInfo.PlatformName);
                            cmd.Parameters.AddWithValue("PlatformVersion", userAgentInfo.PlatformVersion);
                            cmd.Parameters.AddWithValue("ProcessorBits", userAgentInfo.ProcessorBits);
                            cmd.Parameters.AddWithValue("IsMobileDevice", userAgentInfo.IsMobileDevice);
                            cmd.Parameters.AddWithValue("IsTablet", userAgentInfo.IsTablet);
                            cmd.Parameters.AddWithValue("IsSyndicationReader", userAgentInfo.IsSyndicationReader);
                            cmd.Parameters.AddWithValue("IsCrawler", userAgentInfo.IsCrawler);
                            cmd.ExecuteNonQuery();
                        }

                        state.SafeCommit();
                    }
                });
            }
        }

        public override void DeleteAllUserAgentInfo(object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<object>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = "DELETE FROM UserAgentInfoTable;";
                        cmd.ExecuteNonQuery();
                    }

                    state.SafeCommit();
                });
            }
        }

        public override UserAgentInfo GetUserAgentInfo(string userAgentString, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<UserAgentInfo>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = "SELECT Id, UserAgent, BrowserName, BrowserVersion, BrowserVersionMajor, BrowserVersionMinor, BrowserVersionRelease, PlatformName, PlatformVersion, ProcessorBits, IsMobileDevice, IsTablet, IsSyndicationReader, IsCrawler FROM UserAgentInfoTable WHERE UserAgent=@UserAgent;";
                        cmd.Parameters.AddWithValue("UserAgent", userAgentString);
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                state.Result = new UserAgentInfo
                                {
                                    UserAgent = rd.ToNullString("UserAgent", string.Empty),
                                    BrowserName = rd.ToNullString("BrowserName", string.Empty),
                                    BrowserVersion = rd.ToNullString("BrowserVersion", string.Empty),
                                    BrowserVersionMajor = rd.ToNullString("BrowserVersionMajor", string.Empty),
                                    BrowserVersionMinor = rd.ToNullString("BrowserVersionMinor", string.Empty),
                                    BrowserVersionRelease = (UserAgentInfo.BrowserVersionReleaseType)rd.ToNullByte("BrowserVersionRelease", (byte)UserAgentInfo.BrowserVersionReleaseType.Release),
                                    PlatformName = rd.ToNullString("PlatformName", string.Empty),
                                    PlatformVersion = rd.ToNullString("PlatformVersion", string.Empty),
                                    ProcessorBits = (UserAgentInfo.ProcessorBitsType)rd.ToNullByte("ProcessorBits", (byte)UserAgentInfo.ProcessorBitsType.Bit32),
                                    IsMobileDevice = rd.ToNullBool("IsMobileDevice", false),
                                    IsTablet = rd.ToNullBool("IsTablet", false),
                                    IsSyndicationReader = rd.ToNullBool("IsSyndicationReader", false),
                                    IsCrawler = rd.ToNullBool("IsCrawler", false)
                                };
                            }
                        }
                    }
                });

                return mgr.Result;
            }
        }

        public override WebSessionInfo FindWebSessionInfo(string cookieId, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<WebSessionInfo>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = "SELECT Id, UserAgent, IpAddress, CookieId, StartDateTime FROM WebSessionTable WHERE CookieId=@CookieId;";
                        cmd.Parameters.AddWithValue("CookieId", cookieId);
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                state.Result = new WebSessionInfo
                                {
                                    Id = rd.ToNullString("Id", string.Empty),
                                    UserAgent = rd.ToNullString("UserAgent", string.Empty),
                                    IpAddress = rd.ToNullString("IpAddress", string.Empty),
                                    CookieId = rd.ToNullString("CookieId", string.Empty),
                                    StartDateTime = rd.ToNullDateTime("StartDateTime", null)
                                };
                            }
                        }
                    }
                });

                return mgr.Result;
            }
        }

        public void CreateWebSessionInfo(string id, string userAgent, string ipAddress, string cookieId, DateTime actualDateTime, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<object>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = "INSERT INTO WebSessionTable (Id, UserAgent, IpAddress, CookieId, StartDateTime) VALUES(@Id, @UserAgent, @IpAddress, @CookieId, @StartDateTime)";
                        cmd.Parameters.AddWithValue("Id", id);
                        cmd.Parameters.AddWithValue("UserAgent", userAgent);
                        cmd.Parameters.AddWithValue("IpAddress", ipAddress == "::1" ? "127.0.0.1" : ipAddress);
                        cmd.Parameters.AddWithValue("CookieId", cookieId);
                        cmd.Parameters.AddWithValue("StartDateTime", actualDateTime);
                        cmd.ExecuteNonQuery();
                    }

                    state.SafeCommit();
                });
            }
        }

        public override WebSessionInfo EnsureWebSessionInfo(string userAgent, string ipAddress, string cookieId, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<WebSessionInfo>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    var existingWebSession = FindWebSessionInfo(cookieId, state.DataStateObj);
                    if (existingWebSession == null)
                    {
                        existingWebSession = new WebSessionInfo
                        {
                            Id = Guid.NewGuid().ToString(),
                            CookieId = cookieId,
                            IpAddress = ipAddress,
                            StartDateTime = DateTime.Now,
                            UserAgent = userAgent
                        };
                        Debug.Assert(existingWebSession.StartDateTime != null, "existingWebSession.StartDateTime != null");
                        CreateWebSessionInfo(existingWebSession.Id, userAgent, ipAddress, cookieId, existingWebSession.StartDateTime.Value, state.DataStateObj);

                        state.SafeCommit();
                    }
                    else
                    {
                        state.SafeRollback();
                    }

                    state.Result = existingWebSession;
                });

                return mgr.Result;
            }
        }

        public override WebActivityInfo FindWebActivityInfo(string webSessionId, string url, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<WebActivityInfo>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    using (var cmd = CreateCommand((SQLiteTransaction)state.Transaction))
                    {
                        cmd.CommandText = "SELECT Id, WebSessionId, DomainId, Url, ReferralUrl, ActivityDateTime FROM WebActivityTable WHERE WebSessionId=@WebSessionId AND Url=@Url;";
                        cmd.Parameters.AddWithValue("WebSessionId", webSessionId);
                        cmd.Parameters.AddWithValue("Url", url);
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                state.Result = new WebActivityInfo
                                {
                                    Id = rd.ToNullString("Id", string.Empty),
                                    WebSessionId = rd.ToNullString("WebSessionId", string.Empty),
                                    DomainId = rd.ToNullString("DomainId", string.Empty),
                                    Url = rd.ToNullString("Url", string.Empty),
                                    ReferralUrl = rd.ToNullString("ReferralUrl", string.Empty),
                                    ActivityDateTime = rd.ToNullDateTime("ActivityDateTime", null)
                                };
                            }
                        }

                    }
                });

                return mgr.Result;
            }
        }

        private void CreateWebActivityInfo(string id, string webSessionId, string domainId, string url, string referralUrl, DateTime activityDateTime, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<object>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = "INSERT INTO WebActivityTable (Id, WebSessionId, DomainId, Url, ReferralUrl, ActivityDateTime) VALUES(@Id, @WebSessionId, @DomainId, @Url, @ReferralUrl, @ActivityDateTime)";
                        cmd.Parameters.AddWithValue("Id", id);
                        cmd.Parameters.AddWithValue("WebSessionId", webSessionId);
                        cmd.Parameters.AddWithValue("DomainId", domainId);
                        cmd.Parameters.AddWithValue("Url", url);
                        cmd.Parameters.AddWithValue("ReferralUrl", referralUrl);
                        cmd.Parameters.AddWithValue("ActivityDateTime", activityDateTime);
                        cmd.ExecuteNonQuery();
                    }

                    state.SafeCommit();
                });
            }
        }

        public override WebActivityInfo EnsureWebActivityInfo(string webSessionId, string url, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<WebActivityInfo>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    var existingWebActivity = FindWebActivityInfo(webSessionId, url, state.DataStateObj);
                    if (existingWebActivity == null)
                    {
                        var testUrl = new Uri(url);
                        var domain = FindWebDomain(testUrl.Authority, state.DataStateObj);
                        existingWebActivity = new WebActivityInfo
                        {
                            Id = Guid.NewGuid().ToString(),
                            WebSessionId = webSessionId,
                            DomainId = domain.Id,
                            Url = url,
                            ReferralUrl = string.Empty,
                            ActivityDateTime = DateTime.Now
                        };
                        Debug.Assert(existingWebActivity.ActivityDateTime != null, "existingWebActivity.ActivityDateTime != null");
                        CreateWebActivityInfo(existingWebActivity.Id, webSessionId, domain.Id, url, existingWebActivity.ReferralUrl, existingWebActivity.ActivityDateTime.Value, state.DataStateObj);

                        state.SafeCommit();
                    }
                    else
                    {
                        state.SafeRollback();
                    }

                    state.Result = existingWebActivity;
                });

                return mgr.Result;
            }
        }

        public override void CreateWebActivityTiming(string id, string webActivityId, DateTime activityDateTime, string runtimeId, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<object>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = "INSERT INTO WebActivityTimingTable (Id, WebActivityId, ActivityDateTime, RuntimeId) VALUES(@Id, @WebActivityId, @ActivityDateTime, @RuntimeId)";
                        cmd.Parameters.AddWithValue("Id", id);
                        cmd.Parameters.AddWithValue("WebActivityId", webActivityId);
                        cmd.Parameters.AddWithValue("ActivityDateTime", DateTime.Now);
                        cmd.Parameters.AddWithValue("RuntimeId", runtimeId);
                        cmd.ExecuteNonQuery();
                    }

                    state.SafeCommit();
                });
            }
        }

        public override void SaveWebFieldBulk(string webSessionId, List<WebFieldInfo> fields, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<object>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    foreach (var webFieldInfo in fields)
                    {
                        DeleteWebFieldByKey(webFieldInfo.Key, state.DataStateObj);
                        CreateWebFieldInfo(Guid.NewGuid().ToString(),
                            webFieldInfo.WebSessionId,
                            webFieldInfo.WebActivityId,
                            webFieldInfo.Key,
                            webFieldInfo.Value,
                            webFieldInfo.AcrtualDateTime.HasValue ? webFieldInfo.AcrtualDateTime.Value : DateTime.Now,
                            webFieldInfo.DataType,
                            state.DataStateObj);
                    }

                    state.SafeCommit();
                });
            }
        }

        public override void SaveWebFieldBulk(string webSessionId, string webActivityId, List<WebFieldInfo> fields, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<WebActivityInfo>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    foreach (var webFieldInfo in fields)
                    {
                        DeleteWebFieldByKey(webFieldInfo.Key, state.DataStateObj);
                        CreateWebFieldInfo(Guid.NewGuid().ToString(),
                            webFieldInfo.WebSessionId,
                            webFieldInfo.WebActivityId,
                            webFieldInfo.Key,
                            webFieldInfo.Value,
                            webFieldInfo.AcrtualDateTime.HasValue ? webFieldInfo.AcrtualDateTime.Value : DateTime.Now,
                            webFieldInfo.DataType,
                            state.DataStateObj);
                    }

                    state.SafeCommit();
                });
            }
        }

        public void CreateWebFieldInfo(string id, string webSessionId, string webActivityId, string key, string value, DateTime actualDateTime, WebFieldInfo.FieldDataType dataType, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<object>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        if (string.IsNullOrEmpty(webActivityId))
                        {
                            cmd.CommandText = "INSERT INTO WebFieldTable (Id, \"Key\", Value, ActualDateTime, DataType, WebSessionId) VALUES(@Id, @Key, @Value, @ActualDateTime, @DataType, @WebSessionId)";
                        }
                        else
                        {
                            cmd.CommandText = "INSERT INTO WebFieldTable (Id, \"Key\", Value, ActualDateTime, DataType, WebSessionId, WebActivityId) VALUES(@Id, @Key, @Value, @ActualDateTime, @DataType, @WebSessionId, @WebActivityId)";
                            cmd.Parameters.AddWithValue("WebActivityId", webActivityId);
                        }

                        cmd.Parameters.AddWithValue("Id", id);
                        cmd.Parameters.AddWithValue("Key", key);
                        cmd.Parameters.AddWithValue("Value", value);
                        cmd.Parameters.AddWithValue("ActualDateTime", actualDateTime);
                        cmd.Parameters.AddWithValue("DataType", (byte)dataType);
                        cmd.Parameters.AddWithValue("WebSessionId", webSessionId);
                        cmd.ExecuteNonQuery();
                    }

                    state.SafeCommit();
                });
            }
        }

        private void DeleteWebFieldByKey(string key, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<object>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = "DELETE FROM WebFieldTable WHERE Key=@Key";
                        cmd.Parameters.AddWithValue("Key", key);
                        cmd.ExecuteNonQuery();
                    }

                    state.SafeCommit();
                });
            }
        }

        public override List<WebFieldInfo> GetWebFieldsByWebSessionId(string webSessionId, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<List<WebFieldInfo>>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    var result = new List<WebFieldInfo>();

                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = "SELECT Id, \"Key\", Value, ActualDateTime, DataType, WebSessionId FROM WebFieldTable WHERE WebSessionId=@WebSessionId;";
                        cmd.Parameters.AddWithValue("WebSessionId", webSessionId);
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                result.Add(new WebFieldInfo
                                {
                                    Id = rd.ToNullString("Id", string.Empty),
                                    Key = rd.ToNullString("Key", string.Empty),
                                    Value = rd.ToNullString("Value", string.Empty),
                                    WebSessionId = rd.ToNullString("WebSessionId", string.Empty),
                                    AcrtualDateTime = rd.ToNullDateTime("AcrtualDateTime", DateTime.Now),
                                    DataType = (WebFieldInfo.FieldDataType)rd.ToNullByte("DataType", 0)
                                });
                            }
                        }
                    }

                    state.Result = result;
                });

                return mgr.Result;
            }
        }

        public override List<WebFieldInfo> FindWebFieldBulkByWebSession(List<string> keys, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<List<WebFieldInfo>>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    var result = new List<WebFieldInfo>();

                    var inClause = string.Join(",", from string key in keys select string.Format("'{0}'", key));

                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = string.Format("SELECT Id, \"Key\", Value, ActualDateTime, DataType, WebSessionId, WebActivityId FROM WebFieldTable WHERE Key IN ({0});", inClause);
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                result.Add(new WebFieldInfo
                                {
                                    Id = rd.ToNullString("Id", string.Empty),
                                    Key = rd.ToNullString("Key", string.Empty),
                                    Value = rd.ToNullString("Value", string.Empty),
                                    WebSessionId = rd.ToNullString("WebSessionId", string.Empty),
                                    WebActivityId = rd.ToNullString("WebActivityId", null),
                                    AcrtualDateTime = rd.ToNullDateTime("AcrtualDateTime", DateTime.Now),
                                    DataType = (WebFieldInfo.FieldDataType)rd.ToNullByte("DataType", 0)
                                });
                            }
                        }
                    }

                    state.Result = result;
                });

                return mgr.Result;
            }
        }

        public override List<WebFieldInfo> FindWebFieldBulkByWebActivity(List<string> keys, string webActivityId, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<List<WebFieldInfo>>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    var result = new List<WebFieldInfo>();

                    var inClause = string.Join(",", from string key in keys select string.Format("'{0}'", key));

                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = string.Format("SELECT Id, \"Key\", Value, ActualDateTime, DataType, WebSessionId, WebActivityId FROM WebFieldTable WHERE (Key IN ({0})) AND WebActivityId=@WebActivityId;", inClause);
                        cmd.Parameters.AddWithValue("WebActivityId", webActivityId);
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                result.Add(new WebFieldInfo
                                {
                                    Id = rd.ToNullString("Id", string.Empty),
                                    Key = rd.ToNullString("Key", string.Empty),
                                    Value = rd.ToNullString("Value", string.Empty),
                                    WebSessionId = rd.ToNullString("WebSessionId", string.Empty),
                                    WebActivityId = rd.ToNullString("WebActivityId", null),
                                    AcrtualDateTime = rd.ToNullDateTime("AcrtualDateTime", DateTime.Now),
                                    DataType = (WebFieldInfo.FieldDataType)rd.ToNullByte("DataType", 0)
                                });
                            }
                        }
                    }

                    state.Result = result;
                });

                return mgr.Result;
            }
        }

        public override WebDomain FindWebDomain(string domainName, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<WebDomain>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = "SELECT Id, WebSiteName, Domain, CompanyId FROM WebDomainTable WHERE Domain=@Domain";
                        cmd.Parameters.AddWithValue("Domain", domainName);
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                state.Result = new WebDomain
                                {
                                    Id = rd.ToNullString("Id", string.Empty),
                                    WebSiteName = rd.ToNullString("WebSiteName", string.Empty),
                                    Domain = rd.ToNullString("Domain", string.Empty),
                                    CompanyId = rd.ToNullString("CompanyId", string.Empty)
                                };
                            }
                        }
                    }
                });

                return mgr.Result;
            }
        }

        public override LanguageInfo GetLanguageByCompanyId(string companyId, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<LanguageInfo>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = "SELECT Id, Cultures, CompanyId, InternationalName, NativeName, IsDefault FROM LanguageTable WHERE CompanyId=@CompanyId";
                        cmd.Parameters.AddWithValue("CompanyId", companyId);
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                state.Result = new LanguageInfo
                                {
                                    Id = rd.ToNullString("Id", string.Empty),
                                    CompanyId = rd.ToNullString("CompanyId", string.Empty),
                                    Cultures = rd.ToNullString("Cultures", string.Empty),
                                    InternationalName = rd.ToNullString("InternationalName", string.Empty),
                                    NativeName = rd.ToNullString("NativeName", string.Empty)
                                };
                            }
                        }
                    }
                });

                return mgr.Result;
            }
        }

        public override LanguageInfo GetDefaultLanguageByCompanyId(string companyId, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<LanguageInfo>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = "SELECT Id, Cultures, CompanyId, InternationalName, NativeName, IsDefault FROM LanguageTable WHERE CompanyId=@CompanyId AND IsDefault=1";
                        cmd.Parameters.AddWithValue("CompanyId", companyId);
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                state.Result = new LanguageInfo
                                {
                                    Id = rd.ToNullString("Id", string.Empty),
                                    CompanyId = rd.ToNullString("CompanyId", string.Empty),
                                    Cultures = rd.ToNullString("Cultures", string.Empty),
                                    InternationalName = rd.ToNullString("InternationalName", string.Empty),
                                    NativeName = rd.ToNullString("NativeName", string.Empty)
                                };
                            }
                        }
                    }
                });

                return mgr.Result;
            }
        }

        public override LangResourceBarrel GetLangResourceBarrel(List<string> ownerIds, string defaultLanguageId, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<LangResourceBarrel>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    var inClause = string.Join(",", ownerIds);
                    inClause = string.Format("'{0}'", inClause.Replace(",", "','"));

                    if (string.IsNullOrEmpty(defaultLanguageId))
                    {
                        var companyId = GetCompanyIdByOwnerIds(ownerIds, state.DataStateObj);
                        var language = GetDefaultLanguageByCompanyId(companyId, state.DataStateObj);
                        if (language != null)
                            defaultLanguageId = language.Id;
                    }

                    var result = new LangResourceBarrel(defaultLanguageId);
                    var resultItems = new List<LangResource>();

                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = string.Format("SELECT Id, \"Key\", Value, LanguageId, OwnerId FROM LanguageResourceTable WHERE OwnerId IN ({0});", inClause);
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                resultItems.Add(new LangResource
                                {
                                    Id = rd.ToNullString("Id", string.Empty),
                                    Key = rd.ToNullString("Key", string.Empty),
                                    Value = rd.ToNullString("Value", string.Empty),
                                    LanguageId = rd.ToNullString("LanguageId", string.Empty),
                                    OwnerId = rd.ToNullString("OwnerId", null)
                                });
                            }
                        }
                    }

                    result.SetResources(resultItems);
                    state.Result = result;
                });

                return mgr.Result;
            }
        }

        public override Company GetCompanyById(string id, string defaultLanguageId, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<Company>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    var barrel = GetLangResourceBarrel(new List<string> { id }, defaultLanguageId, state.DataStateObj);

                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = "SELECT Id FROM CompanyTable WHERE Id=@Id";
                        cmd.Parameters.AddWithValue("Id", id);
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                var result = new Company
                                {
                                    Id = rd.ToNullString("Id", string.Empty)
                                };
                                result.SetBarrel(barrel);

                                state.Result = result;
                            }
                        }
                    }
                });

                return mgr.Result;
            }
        }

        public override Company GetCompanyByApiKey(string apiKey, string defaultLanguageId, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<Company>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    string companyId;
                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = string.Format(
                            @"SELECT DISTINCT D.CompanyId FROM ApiKeyTable A
                            INNER JOIN ApiKeyRefWebDomainTable REF ON REF.ApiKeyId=A.Id
                            INNER JOIN WebDomainTable D ON D.Id=REF.WebDomainId
                            WHERE A.Key='{0}'
                            LIMIT 1;", apiKey);

                        companyId = cmd.ExecuteScalar() as string;
                    }

                    if (string.IsNullOrEmpty(companyId)) return;

                    var barrel = GetLangResourceBarrel(new List<string> { companyId }, defaultLanguageId, state.DataStateObj);

                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = "SELECT Id FROM CompanyTable WHERE Id=@Id";
                        cmd.Parameters.AddWithValue("Id", companyId);
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                var result = new Company
                                {
                                    Id = rd.ToNullString("Id", string.Empty)
                                };
                                result.SetBarrel(barrel);

                                state.Result = result;
                            }
                        }
                    }
                });

                return mgr.Result;
            }
        }

        public override Company CreateCompany(Company data, object objCnnOrTxn = null)
        {
            if (data == null) return null;
            if (string.IsNullOrEmpty(data.Id)) data.Id = Guid.NewGuid().ToString();

            using (var mgr = new DataContextManager<object>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = "INSERT INTO CompanyTable(Id) VALUES(@Id)";
                        cmd.Parameters.AddWithValue("Id", data.Id);
                        cmd.ExecuteNonQuery();
                    }

                    SaveLangResources(data.GetLangResources(), state.DataStateObj);

                    state.SafeCommit();
                });
            }

            return data;
        }

        public override void DeleteCompany(string id, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<Company>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = "DELETE FROM CompanyTable WHERE Id=@Id";
                        cmd.Parameters.AddWithValue("Id", id);
                        cmd.ExecuteNonQuery();
                    }

                    state.SafeCommit();
                });
            }
        }

        private void SaveLangResources(List<LangResource> langResources, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<object>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    DeleteLangResources(langResources, state.DataStateObj);

                    foreach (var langResource in langResources)
                        InsertLangResource(langResource, state.DataStateObj);

                    state.SafeCommit();
                });
            }
        }

        private void DeleteLangResources(IEnumerable<LangResource> langResources, object objCnnOrTxn = null)
        {
            if (langResources == null) return;

            var ids = from LangResource res in langResources
                      select res.Id;

            DeleteLangResources(ids, objCnnOrTxn);
        }

        private void DeleteLangResources(IEnumerable<string> ids, object objCnnOrTxn = null)
        {
            var inClause = string.Join(",", ids);
            inClause = string.Format("'{0}'", inClause.Replace(",", "','"));

            using (var mgr = new DataContextManager<object>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = string.Format("DELETE FROM LanguageResourceTable WHERE Id IN({0})", inClause);
                        cmd.ExecuteNonQuery();
                    }

                    state.SafeCommit();
                });
            }
        }

        private void InsertLangResource(LangResource langResource, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<object>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = "INSERT INTO LanguageResourceTable(Id, \"Key\", Value, LanguageId, OwnerId) VALUES(@Id, @Key, @Value, @LanguageId, @OwnerId)";
                        cmd.Parameters.AddWithValue("Id", langResource.Id);
                        cmd.Parameters.AddWithValue("Key", langResource.Key);
                        cmd.Parameters.AddWithValue("Value", langResource.Value);
                        cmd.Parameters.AddWithValue("LanguageId", langResource.LanguageId);
                        cmd.Parameters.AddWithValue("OwnerId", langResource.OwnerId);
                        cmd.ExecuteNonQuery();
                    }

                    state.SafeCommit();
                });
            }
        }

        private string GetCompanyIdByOwnerIds(IEnumerable<string> ownerIds, object objCnnOrTxn = null)
        {
            using (var mgr = new DataContextManager<string>(objCnnOrTxn, CreateConnection))
            {
                mgr.Run(state =>
                {
                    var inClause = string.Join(",", ownerIds);
                    inClause = string.Format("'{0}'", inClause.Replace(",", "','"));

                    using (var cmd = CreateCommand(state.DataStateObj))
                    {
                        cmd.CommandText = string.Format(
                            @"SELECT R.CompanyId 
                            FROM (SELECT 
                              L1.*
                            FROM (SELECT DISTINCT 
                              L.CompanyId AS Id 
                            FROM (SELECT 
                              LanguageId 
                            FROM LanguageResourceTable WHERE OwnerId IN({0})) T 
                              INNER JOIN LanguageTable L ON 
                                L.Id=T.LanguageId 
                            LIMIT 1) C 
                              INNER JOIN LanguageTable L1 ON 
                                L1.CompanyId = C.Id
                                ) R
                                WHERE R.IsDefault=1
                            ", inClause);

                        state.Result = cmd.ExecuteScalar() as string;
                    }

                });

                return mgr.Result;
            }
        }

    }
}
