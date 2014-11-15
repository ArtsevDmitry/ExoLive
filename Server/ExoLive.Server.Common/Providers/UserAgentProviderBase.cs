using System.Configuration.Provider;
using ExoLive.Server.Common.Models;

namespace ExoLive.Server.Common.Providers
{
    public abstract class UserAgentProviderBase : ProviderBase
    {
        public abstract int CheckTest(int source);

        public abstract void SetDataProvider(DataProviderBase dataProvider);
        public abstract bool IsNeedDataUpdate();
        public abstract void DataUpdate();
        public abstract UserAgentInfo GetUserAgentInfo(string userAgent);
    }
}
