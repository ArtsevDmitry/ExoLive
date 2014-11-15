using System.Configuration;

namespace ExoLive.Server.Core.Providers.UserAgentProvider.Config
{
    /// <summary>
    /// Data layer provider configuration section
    /// </summary>
    public class UserAgentProviderConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers
        {
            get
            {
                return (ProviderSettingsCollection)base["providers"];
            }
        }

        [ConfigurationProperty("default", DefaultValue = "BrowcapUserAgentProvider")]
        public string DefaultProviderName
        {
            get
            {
                return base["default"] as string;
            }
        }
    }
}
