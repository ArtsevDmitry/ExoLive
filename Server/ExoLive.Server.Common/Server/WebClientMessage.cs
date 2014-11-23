using ExoLive.Server.Common.Models;
using Newtonsoft.Json;

namespace ExoLive.Server.Common.Server
{
    public class WebClientMessage
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("t")]
        public MessageClientCommand Command { get; set; }

        [JsonProperty("d")]
        public string Data { get; set; }

        [JsonIgnore]
        public WebClientContext Context { get; set; }
    }
}
