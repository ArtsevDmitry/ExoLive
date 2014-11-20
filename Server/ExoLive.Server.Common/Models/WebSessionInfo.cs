using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExoLive.Server.Common.Models
{
    public class WebSessionInfo
    {
        public string Id { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
        public string CookieId { get; set; }
        public DateTime? StartDateTime { get; set; }
    }
}
