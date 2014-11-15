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
        public enum OptionSearchPattern : byte
        {
            Equals,
            Begin,
            Contain,
            End
        }

        public abstract int CheckTest(int source);
        public abstract Option SetOption(string key, string value);
        public abstract Option GetOption(string key, string defaultValue = null);
        public abstract List<Option> GetOptions(string keyData, OptionSearchPattern searchPattern, string defaultValue = null);
        public abstract Company GetOrganization(Guid id);
        public abstract ApiKeyInfo GetApiKeyInfo(string apiKey);
    }
}
