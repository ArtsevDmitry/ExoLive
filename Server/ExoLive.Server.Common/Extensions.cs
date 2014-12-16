using System;

namespace ExoLive.Server.Common
{
    public static class Extensions
    {
        public static bool NoCaseCompare(this string source, string dest)
        {
            return string.Compare(source, dest, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}
