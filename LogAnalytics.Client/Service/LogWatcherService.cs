using System;
using System.IO;
using System.Threading.Tasks;

namespace LogAnalytics.Client.Service
{
    public class LogWatcherService : IWatchForLogs, IDisposable
    {
        private readonly string _path;
        private readonly string _filter;
        private FileSystemWatcher _fileSystemWatcher;
        private bool disposedValue;
        private readonly bool _includeSubdirectories;
        public event EventHandler<string> LogFileAdded;

        public LogWatcherService(string path, bool includeSubdirectories = true, string filter = null)
        {
            _path = path;
            _filter = filter ?? "*";
            _includeSubdirectories = includeSubdirectories;
        }

        public void Start()
        {
            Console.WriteLine($"Watching {_path}...");
            _fileSystemWatcher = new FileSystemWatcher(_path, _filter)
            {
                IncludeSubdirectories = _includeSubdirectories
            };
            _fileSystemWatcher.Changed += (s, e) => LogFileAdded?.Invoke(this, e.FullPath);
            _fileSystemWatcher.EnableRaisingEvents = true;
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