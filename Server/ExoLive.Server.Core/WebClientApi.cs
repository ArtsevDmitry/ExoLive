using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ExoLive.Server.Common.Json;
using ExoLive.Server.Common.Models;
using ExoLive.Server.Common.Server;
using ExoLive.Server.Core.Providers.DataProvider;
using Newtonsoft.Json.Linq;

namespace ExoLive.Server.Core
{
    public class WebClientApi
    {
        private static WebClientApi _instance;

        public static WebClientApi Instance
        {
            get
            {
                return _instance ?? (_instance = new WebClientApi());
            }
        }

        public BaseResponseJson WebSessionInit(string userAgent, string ipAddress, string cookieId, string url, Dictionary<string, JValue> caps)
        {
            var wsi = DataProviderManager.Default.EnsureWebSessionInfo(userAgent, ipAddress, cookieId);
            if (wsi == null) return new BaseResponseJson { ResultCode = 1 };

            var wai = DataProviderManager.Default.EnsureWebActivityInfo(wsi.Id, url);
            if (wai == null) return new BaseResponseJson { ResultCode = 1 };

            var qFields = from KeyValuePair<string, JValue> v in caps
                          select new WebFieldInfo
                          {
                              WebSessionId = wsi.Id,
                              AcrtualDateTime = DateTime.Now,
                              DataType = WebFieldInfo.FieldDataType.String,
                              Key = v.Key,
                              Value = v.Value.Value.ToString()
                          };
            DataProviderManager.Default.SaveWebFieldBulk(wsi.Id, qFields.ToList());

            return new BaseResponseJson { ResultCode = 0 };
        }

        public WebClientContext GetClientContext(string userAgent, string ipAddress, string cookieId, string url, ApiKeyInfo apiKey, List<string> userClaims)
        {
            var wsi = DataProviderManager.Default.EnsureWebSessionInfo(userAgent, ipAddress, cookieId);
            var wai = DataProviderManager.Default.EnsureWebActivityInfo(wsi.Id, url);
            return new WebClientContext(wsi, wai, apiKey, userClaims);
        }

        public void NewInMessage(WebClientMessage msg)
        {
            ServerEngine.Instance.PushInMessage(msg);
        }

        public void ProcessInMessage(WebClientMessage msg)
        {
            switch (msg.Command)
            {
                case MessageClientCommand.UserTextMessage:
                    var msgRet = new WebServerMessage
                    {
                        Command = MessageServerCommand.UserTextMessage,
                        Context = msg.Context,
                        Data = string.Format("Ответ: {0}", msg.Data),
                        Id = msg.Id,
                        Number = ServerEngine.Instance.GetNextOutNumber(msg.Context)
                    };
                    ServerEngine.Instance.PushOutMessage(msgRet);
                    break;
            }
            //ServerEngine.Instance.PushOutMessage(msg, webSessionId);
        }

        public List<WebServerMessage> WaitOutMessages(WebClientContext context, long previousSuccessNumber)
        {
            var enterTime = DateTime.Now;
            const int msTimeout = 5000;
            var workTime = 0.0D;
            var result = new List<WebServerMessage>();

            //Register web activity timing
            DataProviderManager.Default.CreateWebActivityTiming(Guid.NewGuid().ToString(), context.WebActivity.Id, enterTime, string.Empty);

            while (workTime < msTimeout)
            {
                if (ServerEngine.Instance.IsOutMessagesExist(context.WebActivity.Id, previousSuccessNumber))
                {
                    var list = ServerEngine.Instance.GetOutMessages(context.WebActivity.Id, previousSuccessNumber);
                    return list;
                }

                Thread.Sleep(workTime > 50 ? 20 : 5);
                workTime = (DateTime.Now - enterTime).TotalMilliseconds;
            }

            return result;
        }



    }
}
