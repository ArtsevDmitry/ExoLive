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
        private ApiKeyInfo _apiKeyInfo;

        public ApiKeyUser(ApiKeyInfo apiKeyInfo)
        {
            ApiKeyInfo = apiKeyInfo;
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
            private set { _apiKeyInfo = value; }
        }
    }
}
