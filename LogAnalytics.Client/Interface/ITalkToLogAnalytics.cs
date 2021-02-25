using System;
using System.Text.Json;
using System.Threading.Tasks;
using LogAnalytics.Client.Model;

namespace LogAnalytics.Client.Service
{
    public interface ITalkToLogAnalytics
    {
        event EventHandler<string> OnError;

        Task<(string Result, string Error)> Flush();
        void Log(LogEntry entry);
    }
}