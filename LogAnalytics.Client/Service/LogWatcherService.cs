using System;
using System.IO;
using System.Threading.Tasks;
using LogAnalytics.Client.Helper;
using LogAnalytics.Client.Model;

namespace LogAnalytics.Client.Service
{
    public class LogWatcherService : IWatchForLogs, IDisposable
    {
        private readonly string _path;
        private readonly string _filter;
        private FileSystemWatcher _fileSystemWatcher;
        private bool disposedValue;
        private readonly bool _includeSubdirectories;
        public event EventHandler<LogAddedEventArgs> LogFileAdded;

        public LogWatcherService(Configuration configuration, bool includeSubdirectories = true, string filter = null)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            if (string.IsNullOrEmpty(configuration.LogDirectory))
            {
                throw new ArgumentNullException("logDirectory");
            }
            _path = configuration.LogDirectory;
            _filter = filter ?? "*";
            _includeSubdirectories = includeSubdirectories;
            Directory.CreateDirectory(GetProcessedFilesDirectory());
        }

        public void Start()
        {
            _fileSystemWatcher = new FileSystemWatcher(_path, _filter)
            {
                IncludeSubdirectories = _includeSubdirectories
            };

            _fileSystemWatcher.Changed += (s, e) => LogFileAdded?.Invoke(this, new LogAddedEventArgs(e.FullPath, FileUtils.GenerateSha256Hash(e.FullPath)));
            _fileSystemWatcher.Error += (s, e) =>
            {
                Console.WriteLine(e.GetException().Message);
                Restart();
            };

            _fileSystemWatcher.EnableRaisingEvents = true;
        }

        public string GetProcessedFilesDirectory(){
            return Path.Combine(Path.GetTempPath(), "processed_logs");
        }
        private void Restart()
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            _fileSystemWatcher.Dispose();
            Start();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _fileSystemWatcher?.Dispose();
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