using System.Configuration.Provider;
using ExoLive.Server.Common.Providers;

namespace ExoLive.Server.Core.Providers.UserAgentProvider.Config
{
    /// <summary>
    /// Data provider collection in configuration file
    /// </summary>
    public class UserAgentProviderCollection : ProviderCollection
    {
        new public UserAgentProviderBase this[string name]
        {
            get { return (UserAgentProviderBase)base[name]; }
        }
    }
}
