using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LogAnalytics.Client.Model
{
    public class Response
    {
        [JsonPropertyName("body")]
        public string Body { get; set; }
        [JsonPropertyName("http_code")]
        public int HttpCode { get; set; }
        [JsonPropertyName("http_headers")]
        public Dictionary<string, string> Headers { get; set; }
    }
}