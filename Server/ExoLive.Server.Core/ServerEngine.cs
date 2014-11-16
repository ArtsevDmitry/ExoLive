using ExoLive.Server.Common;
using ExoLive.Server.Core.Providers.DataProvider;
using ExoLive.Server.Core.Providers.UserAgentProvider;
using ExoLive.Server.Core.RestService;

namespace ExoLive.Server.Core
{
    public class ServerEngine : IServerState
    {
        //Rest Service
        private readonly RestServiceHost _restServiceHost;

        private static ServerEngine _instance;
        public static ServerEngine Instance
        {
            get
            {
                return _instance ?? (_instance = new ServerEngine());
            }
        }

        public ServerEngine()
        {
            _restServiceHost = new RestServiceHost();

            DataProviderManager.Default.CheckTest(100);
            UserAgentProviderManager.Default.CheckTest(100);
            UserAgentProviderManager.Default.SetDataProvider(DataProviderManager.Default);
            if (UserAgentProviderManager.Default.IsNeedDataUpdate())
                UserAgentProviderManager.Default.DataUpdate();

            var usag = UserAgentProviderManager.Default.GetUserAgentInfo(@"Mozilla/5.0 (*FreeBSD x86_64*) Gecko* Firefox/34.0*");
        }

        public void Start()
        {
            _restServiceHost.Start();
        }

        public void Stop()
        {
            _restServiceHost.Stop();
        }

        public ServerState GetCurrentState()
        {
            return _restServiceHost.GetCurrentState();
        }
    }
}
