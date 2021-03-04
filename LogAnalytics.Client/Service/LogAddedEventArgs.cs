using System;

namespace LogAnalytics.Client.Service
{
    public class LogAddedEventArgs : EventArgs
    {
        public LogAddedEventArgs(string filePath)
        {
            FilePath = filePath;
        }
        public string FilePath { get; set; }
    }
}