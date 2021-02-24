using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace LogAnalytics.Client.Helper
{
    public static class SignatureHelper
    {
        public static string CreateSignature(this string json, string workspaceId, string secret, HttpMethod verb, string rfc1123date = null)
        {
            var payloadSize = json.Length;
            if(string.IsNullOrEmpty(rfc1123date)){
                rfc1123date = DateTime.UtcNow.ToString("r");
            }
            var value = $"{verb.Method.ToUpper()}\n{payloadSize}\napplication/json\nx-ms-date:{rfc1123date}\n/api/logs";
            var signedMessage = SignMessage(value, secret);
            return $"{workspaceId}:{signedMessage}";
        }

        public static string SignMessage(string message, string secret)
        {
            var key = Convert.FromBase64String(secret);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            using var hmacsha256 = new HMACSHA256(key);
            var hash = hmacsha256.ComputeHash(messageBytes);
            return Convert.ToBase64String(hash);
        }
    }
}