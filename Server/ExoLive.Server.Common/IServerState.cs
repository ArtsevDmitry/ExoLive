using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExoLive.Server.Common
{
    public enum ServerState
    {
        NotStarted,
        Running
    }
    public interface IServerState
    {
        void Start();
        void Stop();
        ServerState GetCurrentState();
    }
}
