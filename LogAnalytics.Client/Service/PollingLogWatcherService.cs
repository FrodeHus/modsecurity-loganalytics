using System;
using System.IO;
using System.Linq;
using System.Timers;
using LogAnalytics.Client.Helper;

namespace LogAnalytics.Client.Service
{
    public class PollingLogWatcherService : IWatchForLogs, IDisposable
    {
        private bool disposedValue;
        private Timer _poller;
        private readonly string _path;
        private readonly string _processedPath = $"{Path.GetTempPath()}processed_logs";

        public PollingLogWatcherService(string path)
        {
            _path = path;
            Directory.CreateDirectory(GetProcessedFilesDirectory());
        }

        public event EventHandler<LogAddedEventArgs> LogFileAdded;

        public void Start()
        {
            StartPolling();
        }
        public string GetProcessedFilesDirectory()
        {
            return _processedPath;
        }
        private void StartPolling()
        {
            _poller = new Timer
            {
                AutoReset = true,
                Enabled = true,
                Interval = TimeSpan.FromSeconds(10).TotalMilliseconds
            };
            _poller.Elapsed += CheckForNewLogs;
            _poller.Start();
        }

        private void CheckForNewLogs(object sender, ElapsedEventArgs e)
        {
            var files = Directory.EnumerateFiles(_path, "*", new EnumerationOptions
            {
                RecurseSubdirectories = true
            });
            var processedHashes = Directory.EnumerateFiles(GetProcessedFilesDirectory()).Select(f => f[..(f.LastIndexOf(Path.PathSeparator) + 1)]).ToList();
            var unprocessed = files.Select(f => new { File = f, Hash = FileUtils.GenerateSha256Hash(f) })
                                   .Where(p => !processedHashes.Any(h => p.Hash == h));
            foreach (var file in unprocessed)
            {
                LogFileAdded?.Invoke(this, new LogAddedEventArgs(file.File, file.Hash));
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
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