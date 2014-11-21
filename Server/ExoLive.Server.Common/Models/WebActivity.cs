using System;

namespace ExoLive.Server.Common.Models
{
    public class WebActivityInfo
    {
        public string Id { get; set; }
        public string WebSessionId { get; set; }
        public string DomainId { get; set; }
        public string Url { get; set; }
        public string ReferralUrl { get; set; }
        public DateTime? ActivityDateTime { get; set; }
    }
}
