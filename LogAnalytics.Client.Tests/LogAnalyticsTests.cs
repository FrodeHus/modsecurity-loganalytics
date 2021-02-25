using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using LogAnalytics.Client.Helper;
using LogAnalytics.Client.Model;
using Xunit;

namespace LogAnalytics.Client.Tests
{
    public class LogAnalyticsTests
    {
        [Fact]
        public void ItCreatesAValidSignatureForPayloads()
        {
            var date = new DateTime(2021, 02, 20, 0, 0, 0).ToString("r");
            var secret = Convert.ToBase64String(Encoding.UTF8.GetBytes("secret"));
            const string json = @"[{""Uri"":""https://localhost""},{""Uri"":""https://somewhere.local""}]";
            var message = $"POST\n{json.Length}\napplication/json\nx-ms-date:{date}\n/api/logs";
            const string expected = "/B6hLdaYkpP4IKlb9wQJoPsgr1d5IUxdz1nothMv8cU=";
            var actual = SignatureHelper.SignMessage(message, secret);
            actual.Should().Be(expected);
        }

        [Fact]
        public void ItCanDeserializeAJsonLogEntry(){
            var data = File.ReadAllText("../../../data/SampleEntry.json");
            var entry = JsonSerializer.Deserialize<LogEntry>(data);
            entry.Transaction.Should().NotBeNull();
        }
    }
}
