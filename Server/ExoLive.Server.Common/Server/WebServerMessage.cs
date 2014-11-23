using Newtonsoft.Json;

namespace ExoLive.Server.Common.Server
{
    public class WebServerMessage
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("t")]
        public MessageServerCommand Command { get; set; }

        [JsonProperty("d")]
        public string Data { get; set; }
    }
}
