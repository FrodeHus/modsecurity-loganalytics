using System;
using System.Collections.Generic;
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
        public void ItCanDeserializeAJsonLogEntry()
        {
            var data = File.ReadAllText("../../../data/SampleEntry.json");
            var entry = JsonSerializer.Deserialize<LogEntry>(data);
            entry.Transaction.Should().NotBeNull();
        }

        [Fact]
        public void ItFormatsDateTimeToCorrectFormat()
        {
            const string expected = "2021-01-01T13:00:00Z";
            var dateTime = new DateTime(2021, 01, 01, 13, 00, 00);
            var actual = dateTime.ToISO8601();
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("Mon Mar  1 13:00:00 2021")]
        [InlineData("Mon Mar 01 13:00:00 2021")]
        public void ItCanParseModSecDateFormat(string dateValue)
        {
            var expected = new DateTime(2021, 3, 1, 13, 0, 0);
            var actual = dateValue.FromModSecDateTime();
            actual.Should().Be(expected);
        }

        [Fact]
        public void ItCanGenerateLogPathFromDateTime()
        {
            const string expected = "20210101/20210101-1300";
            var date = new DateTime(2021, 01, 01, 13, 0, 0);
            var actual = FileUtils.GenerateLogPathFromDate(date);
            actual.Should().Be(expected);
        }

        [Fact]
        public void ItCanGenerateSha256HashesForFiles()
        {
            const string expected = "089B6121D5A1F9235F1F73F4B14103C5EF5A83A2360CAC90BC405A35C10EC5EF";
            var actual = FileUtils.GenerateSha256Hash("../../../data/SampleEntry.json");
            actual.Should().Be(expected);

        }

        [Fact]
        public void ItCanValidateMd5Sums()
        {
            const string expected = "061a2a82086e33843e5854f0f7354f3d";
            var actual = FileUtils.GenerateMd5Sum("../../../data/SampleEntry.json");
            actual.Should().Be(expected);
        }

        [Fact]
        public void ItCanReadConfigFromFile()
        {
            var expected = new Configuration("123", "secret", "ModSecurity", "/var/log/modsecurity/audit.log");
            var actual = Configuration.FromFile("../../../data/SampleConfig.json");
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ItSerializesHeaderDictionaryToLowercase()
        {
            var headers = new Dictionary<string,string>{
                {"User-Agent", "test"}
            };
            var value = new Request{Headers = headers};
            const string expected = "{\"method\":null,\"http_version\":0,\"uri\":null,\"headers\":{\"user-agent\":\"test\"}}";
            var actual = JsonSerializer.Serialize(value);
            actual.Should().Be(expected);

        }
    }
}
