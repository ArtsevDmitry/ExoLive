﻿using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Data;
using ExoLive.Server.Common.Internationalization;
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
        public abstract List<Option> GetOptions(string keyData, OptionSearchPattern searchPattern, string defaultValue = null, object objCnnOrTxn = null);
        public abstract ApiKeyInfo GetApiKeyInfo(string apiKey, object objCnnOrTxn = null);
        public abstract void InsertUserAgentInfoBulk(List<UserAgentInfo> items, object objCnnOrTxn = null);
        public abstract void DeleteAllUserAgentInfo(object objCnnOrTxn = null);
        public abstract UserAgentInfo GetUserAgentInfo(string userAgentString, object objCnnOrTxn = null);
        public abstract WebSessionInfo FindWebSessionInfo(string cookieId, object objCnnOrTxn = null);
        public abstract WebSessionInfo EnsureWebSessionInfo(string userAgent, string ipAddress, string cookieId, object objCnnOrTxn = null);
        public abstract WebActivityInfo FindWebActivityInfo(string webSessionId, string url, object objCnnOrTxn = null);
        public abstract WebActivityInfo EnsureWebActivityInfo(string webSessionId, string url, object objCnnOrTxn = null);
        public abstract void CreateWebActivityTiming(string id, string webActivityId, DateTime activityDateTime, string runtimeId, object objCnnOrTxn = null);
        public abstract void SaveWebFieldBulk(string webSessionId, List<WebFieldInfo> fields, object objCnnOrTxn = null);
        public abstract void SaveWebFieldBulk(string webSessionId, string webActivityId, List<WebFieldInfo> fields, object objCnnOrTxn = null);
        public abstract List<WebFieldInfo> GetWebFieldsByWebSessionId(string webSessionId, object objCnnOrTxn = null);
        public abstract List<WebFieldInfo> FindWebFieldBulkByWebSession(List<string> keys, object objCnnOrTxn = null);
        public abstract List<WebFieldInfo> FindWebFieldBulkByWebActivity(List<string> keys, string webActivityId, object objCnnOrTxn = null);
        public abstract WebDomain FindWebDomain(string domainName, object objCnnOrTxn = null);
        public abstract LanguageInfo GetLanguageByCompanyId(string companyId, object objCnnOrTxn = null);
        public abstract LanguageInfo GetDefaultLanguageByCompanyId(string companyId, object objCnnOrTxn = null);
        public abstract LangResourceBarrel GetLangResourceBarrel(List<string> ownerIds, string defaultLanguageId, object objCnnOrTxn = null);
        public abstract Company GetCompanyById(string id, string defaultLanguageId, object objCnnOrTxn = null);
        public abstract Company GetCompanyByApiKey(string apiKey, string defaultLanguageId, object objCnnOrTxn = null);
        public abstract Company CreateCompany(Company data, object objCnnOrTxn = null);
        public abstract void DeleteCompany(string id, object objCnnOrTxn = null);
    }
}
