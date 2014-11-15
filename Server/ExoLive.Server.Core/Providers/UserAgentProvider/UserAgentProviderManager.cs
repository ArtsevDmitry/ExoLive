using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using ExoLive.Server.Common.Providers;
using ExoLive.Server.Core.Providers.UserAgentProvider.Config;

namespace ExoLive.Server.Core.Providers.UserAgentProvider
{
    /// <summary>
    /// Data provider manager. Central access point for data layer in server application.
    /// </summary>
    public class UserAgentProviderManager
    {
        static UserAgentProviderManager()
        {
            Initialize();
        }

        private static UserAgentProviderBase _default;
        /// <summary>
        /// Return default data provider from config file
        /// </summary>
        public static UserAgentProviderBase Default
        {
            get { return _default; }
        }

        private static UserAgentProviderCollection _providerCollection;
        /// <summary>
        /// Return collection all available data providers
        /// </summary>
        public static UserAgentProviderCollection Providers
        {
            get { return _providerCollection; }
        }

        private static ProviderSettingsCollection _providerSettings;
        /// <summary>
        /// Return config settings
        /// </summary>
        public static ProviderSettingsCollection ProviderSettings
        {
            get { return _providerSettings; }
        }

        /// <summary>
        /// Parse configuration
        /// </summary>
        private static void Initialize()
        {
            var configSection = (UserAgentProviderConfiguration)ConfigurationManager.GetSection("UserAgentProviders");
            if (configSection == null)
                throw new ConfigurationErrorsException("Section UserAgentProviders not found in config file!");

            _providerCollection = new UserAgentProviderCollection();
            InstantiateProviders(configSection.Providers, _providerCollection, typeof(UserAgentProviderBase));

            _providerSettings = configSection.Providers;

            if (_providerCollection[configSection.DefaultProviderName] == null)
                throw new ConfigurationErrorsException("Default UserAgentProvider not set!");
            _default = _providerCollection[configSection.DefaultProviderName];
        }

        private static void InstantiateProviders(ProviderSettingsCollection configProviders, ProviderCollection providers, Type providerType)
        {
            foreach (ProviderSettings providerSettings in configProviders)
            {
                providers.Add(InstantiateProvider(providerSettings, providerType));
            }
        }

        private static ProviderBase InstantiateProvider(ProviderSettings providerSettings, Type providerType)
        {
            string typeName = providerSettings.Type == null ? null : providerSettings.Type.Trim();
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentException("UserAgentProvider has not type name");

            var splitedTypeName = typeName.Split(',');
            if (splitedTypeName.Length != 2)
                throw new ArgumentException("UserAgentProvider type not contains information about assembly, try use 'typename, assemblyname'");

            var type = Type.GetType(typeName);
            if (type == null)
                throw new ArgumentException("UserAgentProvider type not found");

            if (type.ContainsGenericParameters)
            {
                var genericTypes = providerType.GenericTypeArguments;
                type = type.MakeGenericType(genericTypes);
            }
            if (!providerType.IsAssignableFrom(type))
                throw new ArgumentException("UserAgentProvider must be implemented from type ", providerType.ToString());

            var providerBase = (ProviderBase)Activator.CreateInstance(type, new object[]{});
            var parameters = providerSettings.Parameters;
            var config = new NameValueCollection(parameters.Count, StringComparer.Ordinal);

            foreach (string index in parameters)
            {
                config[index] = parameters[index];
            }

            providerBase.Initialize(providerSettings.Name, config);

            return providerBase;
        }

    }
}
