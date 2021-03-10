using System.Collections.Generic;
using System.Text.Json.Serialization;
using LogAnalytics.Client.Helper;

namespace LogAnalytics.Client.Model
{
    public class Request
    {
        [JsonPropertyName("method")]
        public string Method { get; set; }
        [JsonPropertyName("http_version")]
        public decimal HttpVersion { get; set; }
        [JsonPropertyName("uri")]
        public string Uri { get; set; }
        [JsonPropertyName("headers")]
        [JsonConverter(typeof(HeaderConverter))]
        public Dictionary<string, string> Headers { get; set; }
    }
}