using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ExoLive.Server.Common;
using ExoLive.Server.Common.Models;
using ExoLive.Server.Common.Server;
using ExoLive.Server.Core.Providers.DataProvider;
using ExoLive.Server.Core.Providers.UserAgentProvider;
using ExoLive.Server.Core.RestService;

namespace ExoLive.Server.Core
{
    public class ServerEngine : IServerState
    {
        //Server state variables
        private bool _isEngineWork;
        private bool _isEngineStopPending;
        //Queue processing variables
        private readonly Queue<WebClientMessage> _inQueue;
        private readonly object _inQueueLock = new object();
        private readonly Thread _inQueueThread;
        private readonly Dictionary<string, List<WebServerMessage>> _outDictionary;
        private readonly object _outDictionaryLock = new object();
        private readonly Thread _outDictionaryThread;
        //REST Service
        private readonly RestServiceHost _restServiceHost;
        //Server instance variable
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
            _isEngineWork = false;
            _isEngineStopPending = false;

            DataProviderManager.Default.CheckTest(100);
            UserAgentProviderManager.Default.CheckTest(100);
            UserAgentProviderManager.Default.SetDataProvider(DataProviderManager.Default);
            if (UserAgentProviderManager.Default.IsNeedDataUpdate())
                UserAgentProviderManager.Default.DataUpdate();

            _inQueue = new Queue<WebClientMessage>();
            _outDictionary = new Dictionary<string, List<WebServerMessage>>();
            _inQueueThread = new Thread(InQueueWorkThread);
            _outDictionaryThread = new Thread(OutDictionaryWorkThread);

            _restServiceHost = new RestServiceHost();

        }

        private void SleepIteration(int queueLength)
        {
            if (queueLength >= 20 && queueLength <= 100)
                Thread.Sleep(2);
            else if (queueLength > 0 && queueLength < 20)
                Thread.Sleep(5);
            else if (queueLength == 0)
                Thread.Sleep(50);
        }

        private void InQueueWorkThread()
        {
            while (!_isEngineStopPending)
            {
                var queueLength = _inQueue.Count;
                if (queueLength <= 0)
                {
                    SleepIteration(queueLength);
                    continue;
                }

                var msg = _inQueue.Dequeue();
                WebClientApi.Instance.ProcessInMessage(msg);

                SleepIteration(queueLength);
            }
        }

        private void OutDictionaryWorkThread()
        {
            while (!_isEngineStopPending)
            {
                Thread.Sleep(1000);
            }
        }

        public void Start()
        {
            _isEngineStopPending = false;
            _restServiceHost.Start();
            _inQueueThread.Start();
            _outDictionaryThread.Start();
            _isEngineWork = true;
        }

        public void Stop()
        {
            _isEngineStopPending = true;

            _restServiceHost.Stop();
            _inQueueThread.Join();
            _outDictionaryThread.Join();
            _isEngineWork = false;

            _isEngineStopPending = false;
        }

        public ServerState GetCurrentState()
        {
            return _isEngineWork ? ServerState.Running : ServerState.NotStarted;
        }

        internal void PushInMessage(WebClientMessage msg, WebClientContext webSessionId)
        {
            lock (_inQueueLock)
            {
                if (!CheckInQueueMessageExist(msg.Id))
                    _inQueue.Enqueue(msg);
            }
            Debug.WriteLine("InMessage[{0}]: {1}", msg.Id, msg.Data);
        }

        internal void PushOutMessage(WebServerMessage msg, string webSessionId)
        {
            lock (_outDictionaryLock)
            {
                if (!_outDictionary.ContainsKey(webSessionId))
                {
                    _outDictionary[webSessionId] = new List<WebServerMessage>();
                }

                _outDictionary[webSessionId].Add(msg);
            }
            
            Debug.WriteLine("OutMessage[{0}]: {1}", msg.Id, msg.Data);
        }

        private bool CheckInQueueMessageExist(string id)
        {
            var list = _inQueue.ToList();
            if (list.Any(webClientMessage => webClientMessage.Id.NoCaseCompare(id)))
                return true;

            return false;
        }

        //private int GetInQueueMessagesCount()
        //{
        //    return _inQueue.Count;
        //}

        //private bool CheckOutQueueMessageExist(string id)
        //{
        //    var list = _outQueue.ToList();
        //    if (list.Any(webClientMessage => webClientMessage.Id.NoCaseCompare(id)))
        //        return true;

        //    return false;
        //}


    }
}
