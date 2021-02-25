using System.Text.Json.Serialization;

namespace LogAnalytics.Client.Model
{
    public class Information
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("details")]
        public MessageDetail Details { get; set; }
    }
}