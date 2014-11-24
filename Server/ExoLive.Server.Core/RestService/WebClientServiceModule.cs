using System.Collections.Generic;
using System.Net;
using System.Text;
using ExoLive.Server.Common.Json;
using ExoLive.Server.Common.Models;
using ExoLive.Server.Common.Server;
using Nancy;
using Nancy.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace ExoLive.Server.Core.RestService
{
    public class WebClientServiceModule : NancyModule
    {
        public WebClientServiceModule()
            : base("/webclient/{apikey}")
        {
            this.RequiresAuthentication();

            Post["/wsinit"] = parameters =>
                {
                    var webSessionId = Request.Form["data"];
                    var dicCaps = GetDictionaryFromKeyValueJson(Request.Form["caps"]);
                    BaseResponseJson result = WebClientApi.Instance.WebSessionInit((dicCaps["userAgent"]).Value, Request.UserHostAddress, webSessionId, Request.Headers.Referrer, dicCaps);

                    var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));

                    return new Response
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(jsonBytes, 0, jsonBytes.Length)
                    };
                };

            Post["/in"] = parameters =>
            {
                var context = GetCurrentContext();

                var messagesJson = (DynamicDictionaryValue)Request.Form["data"];
                var messages = JsonConvert.DeserializeObject<List<WebClientMessage>>(messagesJson.HasValue ? messagesJson.Value.ToString() : string.Empty);
                if (messages != null)
                {
                    var sortedMessages = from WebClientMessage msg in messages
                                         orderby msg.Id ascending
                                         select msg;

                    foreach (var webClientMessage in sortedMessages)
                    {
                        webClientMessage.Context = context;
                        WebClientApi.Instance.NewInMessage(webClientMessage);
                    }
                }

                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new BaseResponseJson { ResultCode = 0 }));

                return new Response
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(jsonBytes, 0, jsonBytes.Length)
                };
            };

            Post["/out"] = parameters =>
            {
                var context = GetCurrentContext();
                var previousSuccessNumber = Request.Form["lid"];

                var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(WebClientApi.Instance.WaitOutMessages(context, previousSuccessNumber)));

                return new Response
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(jsonBytes, 0, jsonBytes.Length)
                };
            };

        }

        private WebClientContext GetCurrentContext()
        {
            var webSession = (DynamicDictionaryValue)Request.Form["ws"];
            return WebClientApi.Instance.GetClientContext(Request.Headers.UserAgent, Request.UserHostAddress, webSession.Value.ToString(),
                Request.Headers.Referrer, (ApiKeyInfo)ViewBag.apiKey, (List<string>)ViewBag.userClaims);
        }

        private Dictionary<string, JValue> GetDictionaryFromKeyValueJson(string json)
        {
            var obj = JsonConvert.DeserializeObject(json);
            var dic = new Dictionary<string, JValue>();
            var jobj = obj as JObject;
            if (jobj == null) return dic;

            foreach (var prop in jobj)
            {
                dic.Add(prop.Key, prop.Value as JValue);
            }

            return dic;
        }

    }
}
