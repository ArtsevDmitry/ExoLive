using System;
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
