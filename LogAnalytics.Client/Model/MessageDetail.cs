using System.Text.Json.Serialization;

namespace LogAnalytics.Client.Model
{
    public class MessageDetail
    {
        [JsonPropertyName("match")]
        public string Match { get; set; }
        [JsonPropertyName("reference")]
        public string Reference { get; set; }
        [JsonPropertyName("ruleId")]
        public string RuleId { get; set; }
        [JsonPropertyName("file")]
        public string File { get; set; }
        [JsonPropertyName("lineNumber")]
        public string Line { get; set; }
        [JsonPropertyName("data")]
        public string Data { get; set; }
        [JsonPropertyName("severity")]
        public string Severity { get; set; }
        [JsonPropertyName("ver")]
        public string Version { get; set; }
        [JsonPropertyName("rev")]
        public string Revision { get; set; }
        [JsonPropertyName("tags")]
        public string[] Tags { get; set; }
        [JsonPropertyName("maturity")]
        public string Maturity { get; set; }
        [JsonPropertyName("accuracy")]
        public string Accuracy { get; set; }
    }
}