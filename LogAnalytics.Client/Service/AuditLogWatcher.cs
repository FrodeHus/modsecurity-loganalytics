using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using LogAnalytics.Client.Model;
using Microsoft.Extensions.Logging;

namespace LogAnalytics.Client.Service
{
    ///<summary>
    /// Watches modsec_audit.log for new entries 
    ///</summary>
    public class AuditLogWatcher : IWatchForLogs, IDisposable
    {
        private readonly Configuration _configuration;
        private readonly ILogger _logger;
        private readonly Regex _logMatcher;
        private long _fileMarker;
        private Timer _timer;
        private bool disposedValue;
        private const string _fileMarkerFile = ".auditlogmarker";

        public AuditLogWatcher(Configuration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
            _logMatcher = new(@"(?<file>[^\s][a-z0-9\-\.\/]+) [\d]+ [\d\.]+ md5:(?<md5sum>[a-z0-9]+)", RegexOptions.IgnoreCase);
        }

        public event EventHandler<LogAddedEventArgs> LogFileAdded;

        public void Start()
        {
            var existingMarker = File.ReadAllText(_fileMarkerFile);
            if (long.TryParse(existingMarker, out var existingFileMarker))
            {
                _fileMarker = existingFileMarker;
            }

            _timer = new Timer
            {
                Interval = _configuration.PollingInterval,
                AutoReset = true,
                Enabled = true
            };

            _timer.Elapsed += Poll;
            _timer.Start();
        }

        private void Poll(object sender, ElapsedEventArgs e)
        {
            ParseAuditLog(_configuration.AuditLogFile);
        }

        private void ParseAuditLog(string file)
        {
            using var fileReader = File.OpenRead(file);
            fileReader.Seek(_fileMarker, SeekOrigin.Begin);
            using var reader = new StreamReader(fileReader);
            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                (string filePath, string md5sum) = ParseAuditLogLine(line);
                if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(md5sum))
                {
                    _logger.LogError("Could not find log or md5sum");
                    continue;
                }

                var validLog = ValidateChecksum(filePath, md5sum);
                if (!validLog)
                {
                    _logger.LogError($"Log {filePath} with md5sum {md5sum} did not pass checksum validation");
                    continue;
                }

                LogFileAdded?.Invoke(this, new LogAddedEventArgs(filePath));
                _fileMarker = fileReader.Position;
            }

        }

        ///<summary>
        /// Validates log file against audit log md5 hash to make sure data isn't tampered with
        ///</summary>
        private static bool ValidateChecksum(string logfile, string md5sum)
        {
            var checksum = Encoding.UTF8.GetBytes(md5sum);
            using var hasher = MD5.Create();
            var logHash = hasher.ComputeHash(File.ReadAllBytes(logfile));
            if (logHash == checksum)
            {
                return true;
            }

            return false;
        }

        private (string logFile, string md5sum) ParseAuditLogLine(string line)
        {
            var match = _logMatcher.Match(line);
            if (match.Success)
            {
                return (match.Groups["file"].Value, match.Groups["md5sum"].Value);
            }
            return (null, null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _timer?.Stop();
                    File.WriteAllText(".auditlogmarker", _fileMarker.ToString());
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}