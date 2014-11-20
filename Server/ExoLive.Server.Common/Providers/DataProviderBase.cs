using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Data;
using System.Data.Common;
using ExoLive.Server.Common.Models;

namespace ExoLive.Server.Common.Providers
{
    /// <summary>
    /// Data layer base class
    /// </summary>
    public abstract class DataProviderBase : ProviderBase
    {
        public enum OptionSearchPattern : byte
        {
            Equals,
            Begin,
            Contain,
            End
        }

        public abstract IDbConnection CreateConnection();
        public abstract int CheckTest(int source);
        public abstract Option SetOption(string key, string value, object objCnnOrTxn = null);
        public abstract Option GetOption(string key, string defaultValue = null, object objCnnOrTxn = null);
        public abstract List<Option> GetOptions(string keyData, OptionSearchPattern searchPattern, string defaultValue = null);
        public abstract Company GetOrganization(Guid id);
        public abstract ApiKeyInfo GetApiKeyInfo(string apiKey);
        public abstract void InsertUserAgentInfoBulk(List<UserAgentInfo> items, object objCnnOrTxn = null);
        public abstract void DeleteAllUserAgentInfo(object objCnnOrTxn = null);
        public abstract UserAgentInfo GetUserAgentInfo(string userAgentString, object objCnnOrTxn = null);
        public abstract WebSessionInfo FindWebSessionInfo(string userAgent, string ipAddress, string cookieId, object objCnnOrTxn = null);
        public abstract WebSessionInfo EnsureWebSessionInfo(string userAgent, string ipAddress, string cookieId, object objCnnOrTxn = null);
        public abstract void SaveWebFieldBulk(string webSessionId, List<WebFieldInfo> fields, object objCnnOrTxn = null);
        public abstract List<WebFieldInfo> GetWebFieldsByWebSessionId(string webSessionId, object objCnnOrTxn = null);
        public abstract List<WebFieldInfo> FindWebFieldBulk(List<string> keys, object objCnnOrTxn = null);
        
    }
}
