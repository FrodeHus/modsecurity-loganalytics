using System.Text.Json.Serialization;

namespace LogAnalytics.Client.Model
{
    public class Producer
    {
        [JsonPropertyName("modsecurity")]
        public string ModSecurity { get; set; }
        [JsonPropertyName("connector")]
        public string Connector { get; set; }
        [JsonPropertyName("secrules_engine")]
        public string SecurityRulesEngineEnabled { get; set; }
        [JsonPropertyName("components")]
        public string[] Components { get; set; }
    }
}