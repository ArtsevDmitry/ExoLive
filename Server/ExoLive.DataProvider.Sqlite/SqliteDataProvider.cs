﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using ExoLive.Server.Common.Models;
using ExoLive.Server.Common.Providers;
using System.Linq;

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
            var txn = objCnnOrTxn as IDbTransaction; IDbConnection cnn;
            if (txn != null) cnn = txn.Connection; else cnn = objCnnOrTxn as IDbConnection;
            var selfConnection = cnn == null;
            if (selfConnection) cnn = CreateConnection();
            SQLiteCommand cmd = null;

            var optionResult = new Option
            {
                Id = Guid.NewGuid().ToString(),
                Key = key,
                Value = value
            };

            try
            {
                var existentOption = GetOption(key, "OPTION_NOT_FOUND", objCnnOrTxn);
                if (existentOption.Value == "OPTION_NOT_FOUND")
                {
                    const string sql = @"INSERT INTO OptionTable(Id, ""Key"", Value) VALUES(@Id, @Key, @Value);";
                    cmd = txn != null ? CreateCommand((SQLiteTransaction)txn) : CreateCommand((SQLiteConnection)cnn);
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("Id", optionResult.Id);
                    cmd.Parameters.AddWithValue("Key", optionResult.Key);
                    cmd.Parameters.AddWithValue("Value", optionResult.Value);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    const string sql = @"UPDATE OptionTable SET Value=@Value WHERE Id=@Id;";
                    cmd = txn != null ? CreateCommand((SQLiteTransaction)txn) : CreateCommand((SQLiteConnection)cnn);
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("Id", optionResult.Id);
                    cmd.Parameters.AddWithValue("Key", optionResult.Key);
                    cmd.Parameters.AddWithValue("Value", optionResult.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (selfConnection)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }

            return optionResult;
        }

        public override Option GetOption(string key, string defaultValue = null, object objCnnOrTxn = null)
        {
            var txn = objCnnOrTxn as IDbTransaction; IDbConnection cnn;
            if (txn != null) cnn = txn.Connection; else cnn = objCnnOrTxn as IDbConnection;
            var selfConnection = cnn == null;
            if (selfConnection) cnn = CreateConnection();
            SQLiteCommand cmd = null;

            try
            {
                const string sql = @"SELECT ""Id"", ""Key"", ""Value"" FROM OptionTable WHERE Key='@Key';";
                cmd = txn != null ? CreateCommand((SQLiteTransaction)txn) : CreateCommand((SQLiteConnection)cnn);
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("Key", key);
                using (var rd = cmd.ExecuteReader())
                {
                    if (rd.Read())
                        return new Option
                        {
                            Id = rd.ToNullString("Id", string.Empty),
                            Key = key,
                            Value = rd.ToNullString("Value", defaultValue)
                        };
                }
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (selfConnection)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }

            return new Option
            {
                Id = string.Empty,
                Key = key,
                Value = defaultValue
            };
        }

        public override List<Option> GetOptions(string keyData, OptionSearchPattern searchPattern, string defaultValue = null)
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

            var sql = string.Format(@"SELECT ""Id"", ""Key"", ""Value"" FROM OptionTable WHERE {0};", sqlPattern);
            var results = new List<Option>();
            using (var cnn = CreateConnection())
            {
                using (var cmd = CreateCommand(cnn))
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
            }

            return results;
        }

        public override Company GetOrganization(Guid id)
        {
            return null;
        }

        public override ApiKeyInfo GetApiKeyInfo(string apiKey)
        {
            const string sql = @"SELECT A.Id, A.Key, A.Description, A.IsActive FROM ApiKeyTable A WHERE A.Key=@Key;
SELECT D.Id, D.WebSiteName, D.Domain 
FROM ApiKeyTable A 
INNER JOIN
ApiKeyRefWebDomainTable Ref ON Ref.ApiKeyId=A.Id
INNER JOIN 
WebDomainTable D ON D.Id=Ref.WebDomainId
WHERE A.Key=@Key;";

            var apiKeyInfo = new ApiKeyInfo();

            using (var cnn = CreateConnection())
            {
                using (var cmd = CreateCommand(cnn))
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("Key", apiKey);

                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            apiKeyInfo.Id = rd.ToNullString("Id", string.Empty);
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
                                    Domain = rd.ToNullString("Domain", string.Empty)
                                };
                                apiKeyInfo.WebDomains.Add(webDomain);
                            }
                        }
                    }
                }
            }

            return apiKeyInfo;
        }

        public override void InsertUserAgentInfoBulk(List<UserAgentInfo> items, object objCnnOrTxn = null)
        {
            var txn = objCnnOrTxn as IDbTransaction; IDbConnection cnn;
            if (txn != null) cnn = txn.Connection; else cnn = objCnnOrTxn as IDbConnection;
            var selfConnection = cnn == null;
            if (selfConnection) cnn = CreateConnection();
            SQLiteCommand cmd = null;
            try
            {
                cmd = txn != null ? CreateCommand((SQLiteTransaction)txn) : CreateCommand((SQLiteConnection)cnn);
                cmd.Transaction = (SQLiteTransaction)txn;
                cmd.Connection = (SQLiteConnection)cnn;
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
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (selfConnection)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        public override void DeleteAllUserAgentInfo(object objCnnOrTxn = null)
        {
            var txn = objCnnOrTxn as IDbTransaction; IDbConnection cnn;
            if (txn != null) cnn = txn.Connection; else cnn = objCnnOrTxn as IDbConnection;
            var selfConnection = cnn == null;
            if (selfConnection) cnn = CreateConnection();
            IDbCommand cmd = null;
            try
            {
                cmd = txn != null ? CreateCommand((SQLiteTransaction)txn) : CreateCommand((SQLiteConnection)cnn);
                cmd.Transaction = txn;
                cmd.Connection = cnn;
                cmd.CommandText = "DELETE FROM UserAgentInfoTable;";
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (selfConnection)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        public override UserAgentInfo GetUserAgentInfo(string userAgentString, object objCnnOrTxn = null)
        {
            var txn = objCnnOrTxn as IDbTransaction; IDbConnection cnn;
            if (txn != null) cnn = txn.Connection; else cnn = objCnnOrTxn as IDbConnection;
            var selfConnection = cnn == null;
            if (selfConnection) cnn = CreateConnection();
            SQLiteCommand cmd = null;
            try
            {
                cmd = txn != null ? CreateCommand((SQLiteTransaction)txn) : CreateCommand((SQLiteConnection)cnn);
                cmd.CommandText = "SELECT Id, UserAgent, BrowserName, BrowserVersion, BrowserVersionMajor, BrowserVersionMinor, BrowserVersionRelease, PlatformName, PlatformVersion, ProcessorBits, IsMobileDevice, IsTablet, IsSyndicationReader, IsCrawler FROM UserAgentInfoTable WHERE UserAgent=@UserAgent;";
                cmd.Parameters.AddWithValue("UserAgent", userAgentString);
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        return new UserAgentInfo
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

                return null;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (selfConnection)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        public override WebSessionInfo FindWebSessionInfo(string userAgent, string ipAddress, string cookieId, object objCnnOrTxn = null)
        {
            var txn = objCnnOrTxn as IDbTransaction; IDbConnection cnn;
            if (txn != null) cnn = txn.Connection; else cnn = objCnnOrTxn as IDbConnection;
            var selfConnection = cnn == null;
            if (selfConnection) cnn = CreateConnection();
            SQLiteCommand cmd = null;
            try
            {
                cmd = txn != null ? CreateCommand((SQLiteTransaction)txn) : CreateCommand((SQLiteConnection)cnn);
                cmd.CommandText = "SELECT Id, UserAgent, IpAddress, CookieId, StartDateTime FROM WebSessionTable WHERE UserAgent=@UserAgent AND IpAddress=@IpAddress AND CookieId=@CookieId;";
                cmd.Parameters.AddWithValue("UserAgent", userAgent);
                cmd.Parameters.AddWithValue("IpAddress", ipAddress);
                cmd.Parameters.AddWithValue("CookieId", cookieId);
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        return new WebSessionInfo
                        {
                            Id = rd.ToNullString("Id", string.Empty),
                            UserAgent = rd.ToNullString("UserAgent", string.Empty),
                            IpAddress = rd.ToNullString("IpAddress", string.Empty),
                            CookieId = rd.ToNullString("CookieId", string.Empty),
                            StartDateTime = rd.ToNullDateTime("StartDateTime", null)
                        };
                    }
                }

                return null;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (selfConnection)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        public void CreateWebSessionInfo(string id, string userAgent, string ipAddress, string cookieId, DateTime actualDateTime, object objCnnOrTxn = null)
        {
            var txn = objCnnOrTxn as IDbTransaction; IDbConnection cnn;
            if (txn != null) cnn = txn.Connection; else cnn = objCnnOrTxn as IDbConnection;
            var selfConnection = cnn == null;
            if (selfConnection) cnn = CreateConnection();
            SQLiteCommand cmd = null;
            try
            {
                cmd = txn != null ? CreateCommand((SQLiteTransaction)txn) : CreateCommand((SQLiteConnection)cnn);
                cmd.CommandText = "INSERT INTO WebSessionTable (Id, UserAgent, IpAddress, CookieId, StartDateTime) VALUES(@Id, @UserAgent, @IpAddress, @CookieId, @StartDateTime)";
                cmd.Parameters.AddWithValue("Id", id);
                cmd.Parameters.AddWithValue("UserAgent", userAgent);
                cmd.Parameters.AddWithValue("IpAddress", ipAddress);
                cmd.Parameters.AddWithValue("CookieId", cookieId);
                cmd.Parameters.AddWithValue("StartDateTime", actualDateTime);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (selfConnection)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        public override WebSessionInfo EnsureWebSessionInfo(string userAgent, string ipAddress, string cookieId, object objCnnOrTxn = null)
        {
            var txn = objCnnOrTxn as IDbTransaction; IDbConnection cnn;
            if (txn != null) cnn = txn.Connection; else cnn = objCnnOrTxn as IDbConnection;
            var selfConnection = cnn == null;
            if (selfConnection) cnn = CreateConnection();
            var selfTransaction = false;
            try
            {
                selfTransaction = txn == null;
                if (selfTransaction) txn = cnn.BeginTransaction();

                var existingWebSession = FindWebSessionInfo(userAgent, ipAddress, cookieId, txn);
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
                    Debug.Assert(existingWebSession.StartDateTime != null, "existingWebSession.StartDateTime == null");
                    CreateWebSessionInfo(existingWebSession.Id, userAgent, ipAddress, cookieId, existingWebSession.StartDateTime.Value, txn);

                    txn.Commit();
                }
                else
                {
                    txn.Rollback();
                }

                return existingWebSession;
            }
            finally
            {
                if (selfTransaction && txn != null)
                {
                    txn.Dispose();
                }
                if (selfConnection)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        public override void SaveWebFieldBulk(string webSessionId, List<WebFieldInfo> fields, object objCnnOrTxn = null)
        {
            var txn = objCnnOrTxn as IDbTransaction; IDbConnection cnn;
            if (txn != null) cnn = txn.Connection; else cnn = objCnnOrTxn as IDbConnection;
            var selfConnection = cnn == null;
            if (selfConnection) cnn = CreateConnection();
            var selfTransaction = false;
            try
            {
                selfTransaction = txn == null;
                if (selfTransaction) txn = cnn.BeginTransaction();

                foreach (var webFieldInfo in fields)
                {
                    DeleteWebFieldByKey(webFieldInfo.Key, txn);
                    CreateWebFieldInfo(Guid.NewGuid().ToString(),
                        webFieldInfo.WebSessionId,
                        webFieldInfo.Key,
                        webFieldInfo.Value,
                        webFieldInfo.AcrtualDateTime.HasValue ? webFieldInfo.AcrtualDateTime.Value : DateTime.Now,
                        webFieldInfo.DataType,
                        txn);
                }
                txn.Commit();
            }
            finally
            {
                if (selfTransaction && txn != null)
                {
                    txn.Dispose();
                }
                if (selfConnection)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        public void CreateWebFieldInfo(string id, string webSessionId, string key, string value, DateTime actualDateTime, WebFieldInfo.FieldDataType dataType, object objCnnOrTxn = null)
        {
            var txn = objCnnOrTxn as IDbTransaction; IDbConnection cnn;
            if (txn != null) cnn = txn.Connection; else cnn = objCnnOrTxn as IDbConnection;
            var selfConnection = cnn == null;
            if (selfConnection) cnn = CreateConnection();
            SQLiteCommand cmd = null;
            try
            {
                cmd = txn != null ? CreateCommand((SQLiteTransaction)txn) : CreateCommand((SQLiteConnection)cnn);
                cmd.CommandText = "INSERT INTO WebFieldTable (Id, \"Key\", Value, ActualDateTime, DataType, WebSessionId) VALUES(@Id, @Key, @Value, @ActualDateTime, @DataType, @WebSessionId)";
                cmd.Parameters.AddWithValue("Id", id);
                cmd.Parameters.AddWithValue("Key", key);
                cmd.Parameters.AddWithValue("Value", value);
                cmd.Parameters.AddWithValue("ActualDateTime", actualDateTime);
                cmd.Parameters.AddWithValue("DataType", (byte)dataType);
                cmd.Parameters.AddWithValue("WebSessionId", webSessionId);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (selfConnection)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        private void DeleteWebFieldByKey(string key, object objCnnOrTxn = null)
        {
            var txn = objCnnOrTxn as IDbTransaction; IDbConnection cnn;
            if (txn != null) cnn = txn.Connection; else cnn = objCnnOrTxn as IDbConnection;
            var selfConnection = cnn == null;
            if (selfConnection) cnn = CreateConnection();
            SQLiteCommand cmd = null;
            try
            {
                cmd = txn != null ? CreateCommand((SQLiteTransaction)txn) : CreateCommand((SQLiteConnection)cnn);
                cmd.CommandText = "DELETE FROM WebFieldTable WHERE Key=@Key";
                cmd.Parameters.AddWithValue("Key", key);
                cmd.ExecuteNonQuery();
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (selfConnection)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        public override List<WebFieldInfo> GetWebFieldsByWebSessionId(string webSessionId, object objCnnOrTxn = null)
        {
            var txn = objCnnOrTxn as IDbTransaction; IDbConnection cnn;
            if (txn != null) cnn = txn.Connection; else cnn = objCnnOrTxn as IDbConnection;
            var selfConnection = cnn == null;
            if (selfConnection) cnn = CreateConnection();
            SQLiteCommand cmd = null;
            var result = new List<WebFieldInfo>();
            try
            {
                cmd = txn != null ? CreateCommand((SQLiteTransaction)txn) : CreateCommand((SQLiteConnection)cnn);
                cmd.CommandText =
                    "SELECT Id, \"Key\", Value, ActualDateTime, DataType, WebSessionId FROM WebFieldTable WHERE WebSessionId=@WebSessionId;";
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

                return result;
            }
            catch
            {
                return result;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (selfConnection)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        public override List<WebFieldInfo> FindWebFieldBulk(List<string> keys, object objCnnOrTxn = null)
        {
            var txn = objCnnOrTxn as IDbTransaction; IDbConnection cnn;
            if (txn != null) cnn = txn.Connection; else cnn = objCnnOrTxn as IDbConnection;
            var selfConnection = cnn == null;
            if (selfConnection) cnn = CreateConnection();
            SQLiteCommand cmd = null;
            var result = new List<WebFieldInfo>();
            try
            {
                var inClause = string.Join(",", from string key in keys select string.Format("'{0}'", key));

                cmd = txn != null ? CreateCommand((SQLiteTransaction)txn) : CreateCommand((SQLiteConnection)cnn);
                cmd.CommandText = string.Format("SELECT Id, \"Key\", Value, ActualDateTime, DataType, WebSessionId FROM WebFieldTable WHERE Key IN ({0});", inClause);
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

                return result;
            }
            catch
            {
                return result;
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
                if (selfConnection)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }
    }
}
