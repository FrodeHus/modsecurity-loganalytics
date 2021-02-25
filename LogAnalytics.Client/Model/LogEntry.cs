using System;
using System.Text.Json.Serialization;
using LogAnalytics.Client.Helper;

namespace LogAnalytics.Client.Model
{
    public class LogEntry
    {
        [JsonPropertyName("transaction")]
        public Transaction Transaction { get; set; }
        [JsonPropertyName("time")]
        [JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTime Time
        {
            get
            {
                if(Transaction == null) return default;
                return Transaction.TimeStamp;
            }
        }
    }
}