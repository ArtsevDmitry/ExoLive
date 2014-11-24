using System;

namespace ExoLive.Server.Common.Models
{
    public class WebFieldInfo
    {
        public enum FieldDataType : byte
        {
            None,
            Object,
            Array,
            Constructor,
            Property,
            Comment,
            Integer,
            Float,
            String,
            Boolean,
            Null,
            Undefined,
            Date,
            Raw,
            Bytes,
            Guid,
            Uri,
            TimeSpan 
        }

        public string Id { get; set; }
        public string WebSessionId { get; set; }
        public string WebActivityId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime? AcrtualDateTime { get; set; }
        public FieldDataType DataType { get; set; }
    }
}
