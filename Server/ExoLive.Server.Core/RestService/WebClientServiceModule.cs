using Nancy;
using Nancy.Responses;
using Nancy.Security;

namespace ExoLive.Server.Core.RestService
{
    public class WebClientServiceModule : NancyModule
    {
        public WebClientServiceModule()
            : base("/webclient/{apikey}")
        {
            this.RequiresAuthentication();

            //Before += ctx =>
            //{
            //    return (Context.CurrentUser == null) ? new HtmlResponse(HttpStatusCode.Unauthorized) : null;
            //};

            Get["/test"] = parameters =>
            {
                return "Эй";
            };

            Post["/test"] = parameters =>
            {
                return "Эй";
            };

        }

    }
}
