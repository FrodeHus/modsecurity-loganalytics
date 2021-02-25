using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using LogAnalytics.Client.Helper;

namespace LogAnalytics.Client.Model
{
    public class Transaction
    {
        [JsonPropertyName("client_ip")]
        public string ClientIp { get; set; }
        [JsonPropertyName("time_stamp")]
        [JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTime TimeStamp { get; set; }
        [JsonPropertyName("server_id")]
        public string ServerId { get; set; }
        [JsonPropertyName("client_port")]
        public int ClientPort { get; set; }
        [JsonPropertyName("host_ip")]
        public string HostIp { get; set; }
        [JsonPropertyName("host_port")]
        public int HostPort { get; set; }
        [JsonPropertyName("unique_id")]
        public string UniqueId { get; set; }
        [JsonPropertyName("request")]
        public Request Request { get; set; }
        [JsonPropertyName("response")]
        public Response Response { get; set; }
        [JsonPropertyName("messages")]
        public Information[] Messages { get; set; }
        [JsonPropertyName("producer")]
        public Producer Producer { get; set; }
    }
}