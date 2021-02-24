using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using LogAnalytics.Client.Helper;
using LogAnalytics.Client.Service;
using NSubstitute;
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
        public async Task ItValidatesJsonBeforeSending()
        {
            var client = Substitute.For<HttpClient>();
            var service = new LogAnalyticsService(client, "testlog", "123", "secret");
            var (_, Error) = await service.PostData("asdf").ConfigureAwait(false);
            Error.Should().StartWith("JSON is not valid");
        }
    }
}
