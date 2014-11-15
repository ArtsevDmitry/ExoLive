using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SQLite;
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

        public override int CheckTest(int source)
        {
            return source;
        }

        public override Option SetOption(string key, string value)
        {
            var optionResult = new Option
            {
                Id = Guid.NewGuid().ToString(),
                Key = key,
                Value = value
            };
            const string sql = @"INSERT INTO OptionTable(Id, ""Key"", Value) VALUES(@Id, @Key, @Value);";
            using (var cnn = CreateConnection())
            {
                using (var cmd = CreateCommand(cnn))
                {
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("Id", optionResult.Id);
                    cmd.Parameters.AddWithValue("Key", optionResult.Key);
                    cmd.Parameters.AddWithValue("Value", optionResult.Value);
                    cmd.ExecuteNonQuery();
                }
            }

            return optionResult;
        }

        public override Option GetOption(string key, string defaultValue = null)
        {
            const string sql = @"SELECT ""Id"", ""Key"", ""Value"" FROM OptionTable WHERE Key='@Key';";

            using (var cnn = CreateConnection())
            {
                using (var cmd = CreateCommand(cnn))
                {
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
    }
}
