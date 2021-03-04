using System;

namespace LogAnalytics.Client.Service
{
    public interface IWatchForLogs
    {
        event EventHandler<LogAddedEventArgs> LogFileAdded;

        void Start();
    }
}