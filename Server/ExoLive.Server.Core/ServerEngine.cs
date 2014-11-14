using ExoLive.Server.Common;
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
