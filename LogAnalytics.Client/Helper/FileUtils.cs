using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LogAnalytics.Client.Helper
{
    public static class FileUtils
    {
        public static string GenerateLogPathFromDate(DateTime date)
        {
            var day = date.ToString("yyyyMMdd");
            var time = date.ToString("HHmm");
            return Path.Combine(day, $"{day}-{time}");
        }

        public static string GenerateSha256Hash(string file)
        {
            using var stream = File.OpenRead(file);
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(stream);
            var builder = new StringBuilder();
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }
    }
}