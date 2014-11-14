using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Reflection;
using ExoLive.Server.Common.Providers;
using ExoLive.Server.Core.Providers.DataProvider.Config;

namespace ExoLive.Server.Core.Providers.DataProvider
{
    /// <summary>
    /// Data provider manager. Central access point for data layer in server application.
    /// </summary>
    public class DataProviderManager
    {
        static DataProviderManager()
        {
            Initialize();
        }

        private static DataProviderBase _default;
        /// <summary>
        /// Return default data provider from config file
        /// </summary>
        public static DataProviderBase Default
        {
            get { return _default; }
        }

        private static DataProviderCollection _providerCollection;
        /// <summary>
        /// Return collection all available data providers
        /// </summary>
        public static DataProviderCollection Providers
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
            var configSection = (DataProviderConfiguration)ConfigurationManager.GetSection("DataProviders");
            if (configSection == null)
                throw new ConfigurationErrorsException("Section DataProvider not found in config file!");

            _providerCollection = new DataProviderCollection();
            InstantiateProviders(configSection.Providers, _providerCollection, typeof(DataProviderBase));

            _providerSettings = configSection.Providers;

            if (_providerCollection[configSection.DefaultProviderName] == null)
                throw new ConfigurationErrorsException("DataProvider по умолчанию не задан!");
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
                throw new ArgumentException("DataProvider has not type name");

            var splitedTypeName = typeName.Split(',');
            if (splitedTypeName.Length != 2)
                throw new ArgumentException("DataProvider type not contains information about assembly, try use 'typename, assemblyname'");

            //var asmProvider = Assembly.Load(splitedTypeName[1].Trim());
            //try
            //{
            //    var alltypes = asmProvider.GetTypes();
            //}
            //catch (Exception ex)
            //{
                
            //}
            var type = Type.GetType(typeName);//, "type", providerSettings, null, true);
            if (type == null)
                throw new ArgumentException("DataProvider type not found");

            if (type.ContainsGenericParameters)
            {
                var genericTypes = providerType.GenericTypeArguments;
                type = type.MakeGenericType(genericTypes);
            }
            if (!providerType.IsAssignableFrom(type))
                throw new ArgumentException("DataProvider must be implemented from type ", providerType.ToString());

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
