using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LogAnalytics.Client.Helper;

namespace LogAnalytics.Client.Service
{
    public class LogAnalyticsService : ITalkToLogAnalytics
    {
        public event EventHandler<string> OnError;
        private readonly HttpClient _httpClient;
        private readonly string _logName;
        private readonly string _workspaceId;
        private readonly string _sharedAccessKey;
        private readonly string _url;
        private readonly List<JsonElement> _buffer = new();
        private System.Timers.Timer _timer;

        public LogAnalyticsService(HttpClient httpClient, string logName, string workspaceId, string sharedAccessKey)
        {
            _url = "https://" + workspaceId + ".ods.opinsights.azure.com/api/logs?api-version=2016-04-01";
            _httpClient = httpClient;
            _logName = logName;
            _workspaceId = workspaceId;
            _sharedAccessKey = sharedAccessKey;
            InitializeClient();
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            _timer = new System.Timers.Timer
            {
                Interval = TimeSpan.FromMinutes(2).TotalMilliseconds,
                AutoReset = true,
                Enabled = true
            };

            _timer.Elapsed += async (s, e) =>
            {
                (_, var Error) = await Flush().ConfigureAwait(false);
                if (Error != null)
                {
                    OnError?.Invoke(this, Error);
                }
            };
            _timer.Start();
        }

        private void InitializeClient()
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("Log-Type", _logName);
            _httpClient.DefaultRequestHeaders.Add("time-generated-field", "transaction_time_stamp_s");
        }

        public void Log(JsonElement entry)
        {
            _buffer.Add(entry);
        }

        /// <summary>
        /// Sends log data to Log Analytics in JSON-format
        /// </summary>
        /// <param name="json"></param>
        public async Task<(string Result, string Error)> Flush()
        {
            if (_buffer.Count == 0) return (null, null);

            try
            {
                var json = JsonSerializer.Serialize(_buffer);
                var date = DateTime.UtcNow.ToString("r");
                var message = new HttpRequestMessage(HttpMethod.Post, new Uri(_url));
                var authorizationHeader = json.CreateSignature(_workspaceId, _sharedAccessKey, HttpMethod.Post, date);
                var httpContent = new StringContent(json, Encoding.UTF8);
                message.Headers.Add("Authorization", $"SharedKey {authorizationHeader}");
                httpContent.Headers.Add("x-ms-date", date);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                message.Content = httpContent;
                var response = await _httpClient.SendAsync(message).ConfigureAwait(false);
                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    _buffer.Clear();
                    return (result, null);
                }

                return (null, result);
            }
            catch (JsonException je)
            {
                return (null, "JSON is not valid: " + je.Message);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }
    }
}