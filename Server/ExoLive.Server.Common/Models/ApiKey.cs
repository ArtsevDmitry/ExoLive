using System.Collections.Generic;

namespace ExoLive.Server.Common.Models
{
    public class ApiKey
    {
        public string Id { get; set; }
        public string CompanyId { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
