using System;

namespace LogAnalytics.Client.Service
{
    public class LogAddedEventArgs : EventArgs
    {
        public LogAddedEventArgs(string filePath, string fileHash)
        {
            FilePath = filePath;
            FileHash = fileHash;
        }
        public string FilePath { get; set; }
        public string FileHash { get; set; }
    }
}