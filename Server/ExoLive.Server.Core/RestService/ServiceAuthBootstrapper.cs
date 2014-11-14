using System;
using System.Linq;
using ExoLive.Server.Core.Providers.DataProvider;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace ExoLive.Server.Core.RestService
{
    public class ServiceAuthBootstrapper : DefaultNancyBootstrapper
    {

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {
            var configuration =
                new StatelessAuthenticationConfiguration(nancyContext =>
                {
                    //Find ApiKey in url string
                    var path = nancyContext.Request.Path;
                    var pathSegments = path.Split('/');
                    string apiKey = string.Empty;
                    for (var i = 0; i < pathSegments.Length; i++)
                    {
                        var segment = pathSegments[i].Trim();
                        if (segment.NoCaseCompare("webclient") && (i + 1) <= pathSegments.Length)
                        {
                            apiKey = pathSegments[i + 1];
                            break;
                        }
                    }

                    //Get url of referral web site
                    Uri originalUrl;
                    if (Uri.TryCreate(nancyContext.Request.Headers.Referrer, UriKind.Absolute, out originalUrl))
                    {
                        return GetApiKeyUser(apiKey, originalUrl.Authority);
                    }

                    //Base parameters not found, user is not authenticated
                    return null; 
                });

            AllowAccessToConsumingSite(pipelines);

            StatelessAuthentication.Enable(pipelines, configuration);
        }

        static void AllowAccessToConsumingSite(IPipelines pipelines)
        {
            pipelines.AfterRequest.AddItemToEndOfPipeline(x =>
            {
                x.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                x.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,DELETE,PUT,OPTIONS");
            });
        }

        private ApiKeyUser GetApiKeyUser(string apiKey, string domain)
        {
            var apiKeyInfo = DataProviderManager.Default.GetApiKeyInfo(apiKey);
            if (apiKeyInfo == null || !apiKeyInfo.WebDomains.Any())
                return null;

            if (!apiKeyInfo.IsDomainExist(domain))
                return null;

            return new ApiKeyUser(apiKeyInfo);
        }

    }
}
