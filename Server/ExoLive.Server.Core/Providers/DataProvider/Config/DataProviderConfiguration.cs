using System.Configuration;

namespace ExoLive.Server.Core.Providers.DataProvider.Config
{
    /// <summary>
    /// Data layer provider configuration section
    /// </summary>
    public class DataProviderConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers
        {
            get
            {
                return (ProviderSettingsCollection)base["providers"];
            }
        }

        [ConfigurationProperty("default", DefaultValue = "SqliteDataProvider")]
        public string DefaultProviderName
        {
            get
            {
                return base["default"] as string;
            }
        }
    }
}
