using System;
using ExoLive.Server.Common;
using Nancy.Hosting.Self;

namespace ExoLive.Server.Core.RestService
{
    public class RestServiceHost : IServerState
    {
        private readonly NancyHost _host;
        private ServerState _serverState = ServerState.NotStarted;

        public RestServiceHost()
        {
            _host = new NancyHost(new Uri("http://localhost:7777"));
        }

        public void Start()
        {
            _host.Start();
            _serverState = ServerState.Running;
        }

        public void Stop()
        {
            _host.Stop();
            _serverState = ServerState.NotStarted;
        }

        public ServerState GetCurrentState()
        {
            return _serverState;
        }
    }
}
