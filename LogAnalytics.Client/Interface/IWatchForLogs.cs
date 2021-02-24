using System;

namespace LogAnalytics.Client.Service
{
    public interface IWatchForLogs
    {
        event EventHandler<string> LogFileAdded;

        void Start();
    }
}