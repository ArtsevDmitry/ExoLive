using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ExoLive.Server.Common.Models
{
    public class LanguageInfo
    {
        public string Id { get; set; }
        public string CompanyId { get; set; }
        public string Cultures { get; set; }
        public string InternationalName { get; set; }
        public string NativeName { get; set; }

        public List<string> CultureCodes
        {
            get
            {
                if (string.IsNullOrEmpty(Cultures)) return new List<string>();

                var result = Cultures.Split(',');
                return (from string r in result
                        select r.Trim()).ToList();
            }
        }

        public List<CultureInfo> ValidCultures
        {
            get
            {
                return (from CultureInfo c in
                            from cultureCode in CultureCodes
                            select CultureInfo.GetCultureInfo(cultureCode)
                        where c != null
                        select c).ToList();
            }
        }

    }
}
