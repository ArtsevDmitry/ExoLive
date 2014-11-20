using System.Collections.Generic;
using System.Text;
using ExoLive.Server.Common.Json;
using Nancy;
using Nancy.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                    BaseResponseJson result = WebClientApi.Instance.WebSessionInit((dicCaps["userAgent"]).Value, Request.UserHostAddress, webSessionId, dicCaps);

                    var jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result));

                    return new Response
                    {
                        ContentType = "application/json",
                        Contents = s => s.Write(jsonBytes, 0, jsonBytes.Length)
                    };
                };

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
