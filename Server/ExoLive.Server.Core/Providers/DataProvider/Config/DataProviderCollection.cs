using System.Configuration.Provider;
using ExoLive.Server.Common.Providers;

namespace ExoLive.Server.Core.Providers.DataProvider.Config
{
    /// <summary>
    /// Data provider collection in configuration file
    /// </summary>
    public class DataProviderCollection : ProviderCollection
    {
        new public DataProviderBase this[string name]
        {
            get { return (DataProviderBase)base[name]; }
        }
    }
}
