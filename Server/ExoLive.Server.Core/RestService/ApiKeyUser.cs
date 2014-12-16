using System.Collections.Generic;
using ExoLive.Server.Common.Models;
using Nancy.Security;

namespace ExoLive.Server.Core.RestService
{
    /// <summary>
    /// User authenticated in service by API key
    /// </summary>
    public class ApiKeyUser : IUserIdentity
    {
        private readonly List<string> _claims = new List<string>();
        private readonly ApiKeyInfo _apiKeyInfo;
        private readonly Company _company;
        private readonly LanguageInfo _defaultLanguage;

        public ApiKeyUser(ApiKeyInfo apiKeyInfo, Company company, LanguageInfo defaultLanguage)
        {
            _apiKeyInfo = apiKeyInfo;
            _company = company;
            _defaultLanguage = defaultLanguage;
            _claims.Add(string.Format("ApiKey={0}", _apiKeyInfo.Key));
        }

        public string UserName
        {
            get { return _apiKeyInfo != null ? _apiKeyInfo.Key : string.Empty; }
        }

        public IEnumerable<string> Claims
        {
            get { return _claims; }
        }

        public ApiKeyInfo ApiKeyInfo
        {
            get { return _apiKeyInfo; }
        }

        public Company Company
        {
            get { return _company; }
        }

        public LanguageInfo DefaultLanguage
        {
            get { return _defaultLanguage; }
        }

    }
}
