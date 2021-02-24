using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace LogAnalytics.Client.Service
{
    public interface ITalkToLogAnalytics
    {
        event EventHandler<string> OnError;

        Task<(string Result, string Error)> Flush();
        void Log(JsonElement entry);
    }
}