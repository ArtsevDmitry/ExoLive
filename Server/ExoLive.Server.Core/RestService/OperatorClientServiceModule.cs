using System.Globalization;
using System.Text;
using System.Threading;
using ExoLive.Server.Common.Json;
using Nancy;
using Newtonsoft.Json;

namespace ExoLive.Server.Core.RestService
{
    public class OperatorClientServiceModule : NancyModule
    {
        public OperatorClientServiceModule()
            : base("/operatorclient")
        {

            Post["/auth"] = parameters =>
            {
                Thread.Sleep(5000);
                var loginData = (DynamicDictionaryValue)Request.Form["l"];
                var password = ((DynamicDictionaryValue)Request.Form["p"]).ToString(CultureInfo.InvariantCulture);
                var splitedLoginData = loginData.Value.ToString().Split('/');
                string login = null, domain = null;
                byte[] jsonBytes = null;
                if (splitedLoginData.Length == 2)
                {
                    domain = splitedLoginData[0];
                    login = splitedLoginData[1];
                }

                //TODO: Test section (need remove later)
                {
                    if (!(domain.NoCaseCompare("test") && login.NoCaseCompare("user") && password.NoCaseCompare("123")))
                        jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new BaseResponseJson { ResultCode = 255 }));
                }

                jsonBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new BaseResponseJson { ResultCode = 0 }));

                return new Response
                {
                    ContentType = "application/json",
                    Contents = s => s.Write(jsonBytes, 0, jsonBytes.Length)
                };
            };
        }
    }
}
