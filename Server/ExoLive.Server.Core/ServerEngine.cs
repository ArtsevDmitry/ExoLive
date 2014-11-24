using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
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
        //private readonly Thread _outDictionaryThread;
        private readonly Dictionary<string, long> _outIdDictionary;
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
            //_outDictionaryThread = new Thread(OutDictionaryWorkThread);
            _outIdDictionary = new Dictionary<string, long>();

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

        //private void OutDictionaryWorkThread()
        //{
        //    while (!_isEngineStopPending)
        //    {
        //        Thread.Sleep(1000);
        //    }
        //}

        public void Start()
        {
            _isEngineStopPending = false;
            _restServiceHost.Start();
            _inQueueThread.Start();
            //_outDictionaryThread.Start();
            _isEngineWork = true;
        }

        public void Stop()
        {
            _isEngineStopPending = true;

            _restServiceHost.Stop();
            _inQueueThread.Join();
            //_outDictionaryThread.Join();
            _isEngineWork = false;

            _isEngineStopPending = false;
        }

        public ServerState GetCurrentState()
        {
            return _isEngineWork ? ServerState.Running : ServerState.NotStarted;
        }

        internal void PushInMessage(WebClientMessage msg)
        {
            lock (_inQueueLock)
            {
                if (!CheckInQueueMessageExist(msg.Id))
                    _inQueue.Enqueue(msg);
            }
            Debug.WriteLine("InMessage[{0}]: {1}", msg.Id, msg.Data);
        }

        internal void PushOutMessage(WebServerMessage msg)
        {
            lock (_outDictionaryLock)
            {
                if (!_outDictionary.ContainsKey(msg.Context.WebActivity.Id))
                {
                    _outDictionary[msg.Context.WebActivity.Id] = new List<WebServerMessage>();
                }

                _outDictionary[msg.Context.WebActivity.Id].Add(msg);
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

        internal long GetNextOutNumber(WebClientContext context)
        {
            const string lastOutNumberConst = "LastOutNumber";
            long newNum = 0;
            using (var cnn = DataProviderManager.Default.CreateConnection())
            {
                using (var txn = cnn.BeginTransaction())
                {
                    var fields = DataProviderManager.Default.FindWebFieldBulkByWebActivity(new List<string> { lastOutNumberConst }, context.WebActivity.Id, txn);
                    var qField = from WebFieldInfo field in fields
                                 where field.Key == lastOutNumberConst
                                 select field;
                    var fieldLastOutNumber = qField.FirstOrDefault();
                    long num = 0;
                    if (fieldLastOutNumber != null)
                    {
                        num = Convert.ToInt64(fieldLastOutNumber.Value);
                    }
                    newNum = num + 1;
                    DataProviderManager.Default.SaveWebFieldBulk(context.WebSession.Id, context.WebActivity.Id, new List<WebFieldInfo>
                    {
                        new WebFieldInfo
                        {
                            AcrtualDateTime = DateTime.Now,
                            DataType = WebFieldInfo.FieldDataType.String,
                            Key = lastOutNumberConst,
                            Value = Convert.ToString(newNum),
                            WebSessionId = context.WebSession.Id,
                            WebActivityId = context.WebActivity.Id
                        }
                    }, txn);

                    txn.Commit();
                }
            }

            return newNum;
            //lock (_outIdDictionary)
            //{
            //    if (_outIdDictionary.ContainsKey(id)) return ++_outIdDictionary[id];
            //    _outIdDictionary[id] = 0;
            //    return 0;
            //}
        }

        internal bool IsOutMessagesExist(string id, long previousSuccessNumber)
        {
            lock (_outDictionaryLock)
            {
                if (!_outDictionary.ContainsKey(id)) return false;

                var list = _outDictionary[id];
                if (list == null || !list.Any()) return false;
                var qNewMessageTest = from WebServerMessage msg in list
                                      where msg.Number > previousSuccessNumber
                                      select msg;
                return qNewMessageTest.Any();
            }
        }

        internal List<WebServerMessage> GetOutMessages(string id, long previousSuccessNumber)
        {
            lock (_outDictionaryLock)
            {
                var list = _outDictionary[id];
                if (list != null)
                {
                    list.RemoveAll(x => x.Number < previousSuccessNumber);

                    var qSortedList = from WebServerMessage msg in list
                                      where msg.Number > previousSuccessNumber
                                      orderby msg.Number ascending
                                      select msg;

                    return qSortedList.ToList();
                }
            }

            return null;
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
