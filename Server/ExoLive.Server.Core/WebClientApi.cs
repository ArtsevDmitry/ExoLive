using System;
using System.Collections.Generic;
using System.Linq;
using ExoLive.Server.Common.Json;
using ExoLive.Server.Common.Models;
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

    }
}
