using System.Collections.Generic;

namespace ExoLive.Server.Common.Models
{
    public class WebClientContext
    {
        public WebClientContext(WebSessionInfo webSession, WebActivityInfo webActivity, ApiKeyInfo apiKey, List<string> userClaims)
        {
            WebSession = webSession;
            WebActivity = webActivity;
            ApiKey = apiKey;
            UserClaims = userClaims;
        }

        public WebSessionInfo WebSession { get; private set; }
        public WebActivityInfo WebActivity { get; private set; }
        public ApiKeyInfo ApiKey { get; private set; }
        public List<string> UserClaims { get; private set; }

        public bool IsValid
        {
            get { return WebSession != null && WebActivity != null && ApiKey != null && UserClaims != null; }
        }
    }
}
