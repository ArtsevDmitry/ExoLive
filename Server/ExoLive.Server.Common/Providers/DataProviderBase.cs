using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using ExoLive.Server.Common.Models;

namespace ExoLive.Server.Common.Providers
{
    /// <summary>
    /// Data layer base class
    /// </summary>
    public abstract class DataProviderBase : ProviderBase
    {
        public abstract Company GetOrganization(Guid id);

        public abstract ApiKeyInfo GetApiKeyInfo(string apiKey);
    }
}
