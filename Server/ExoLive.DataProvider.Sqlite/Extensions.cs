using System.Data.Common;

namespace ExoLive.DataProvider.SqLite
{
    public static class Extensions
    {

        public static string ToNullString(this DbDataReader rd, string columnName, string defaultValue)
        {
            var index = rd.GetOrdinal(columnName);
            return rd.IsDBNull(index) ? defaultValue : rd.GetString(index);
        }

        public static bool ToNullBool(this DbDataReader rd, string columnName, bool defaultValue)
        {
            var index = rd.GetOrdinal(columnName);
            return rd.IsDBNull(index) ? defaultValue : rd.GetBoolean(index);
        }

    }
}
