using System;
using System.Collections.Generic;
using System.Linq;

namespace ExoLive.Server.Common.Models
{
    public class ApiKeyInfo : ApiKey
    {
        public ApiKeyInfo()
        {
            WebDomains = new List<WebDomain>();
        }

        public List<WebDomain> WebDomains { get; private set; }

        public bool IsDomainExist(string domain)
        {
            return (from WebDomain d in WebDomains
                    where string.Compare(d.Domain, domain, StringComparison.InvariantCultureIgnoreCase) == 0
                    select d).Any();
        }
    }
}
