using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExoLive.Server.Common.Models
{
    public class UserAgentInfo
    {
        public enum ProcessorBitsType : byte
        {
            Bit16 = 16,
            Bit32 = 32,
            Bit64 = 64
        }

        public enum BrowserVersionReleaseType : byte
        {
            Alpha = 2,
            Beta = 1,
            Release = 0
        }

        public string UserAgent { get; set; }
        public string BrowserName { get; set; }
        public string BrowserVersion { get; set; }
        public string BrowserVersionMajor { get; set; }
        public string BrowserVersionMinor { get; set; }
        public BrowserVersionReleaseType BrowserVersionRelease { get; set; }
        public string PlatformName { get; set; }
        public string PlatformVersion { get; set; }
        public ProcessorBitsType ProcessorBits { get; set; }
        public bool IsMobileDevice { get; set; }
        public bool IsTablet { get; set; }
        public bool IsSyndicationReader { get; set; }
        public bool IsCrawler { get; set; }

    }
}
