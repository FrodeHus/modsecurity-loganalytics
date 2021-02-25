using System;
using System.IO;
using CommandLine;
using LogAnalytics.Client.Service;
using System.Text.Json;
namespace ModSecurityLogger
{
    class Program
    {
        public class Options
        {
            [Option('w', "workspace", Required = false, HelpText = "ID of your workspace")]
            public string WorkspaceID { get; set; }
            [Option('l', "log", Required = true, HelpText = "Name of the datasource/log you wish to send data to")]
            public string LogName { get; set; }
            [Option('k', "key", Required = false, HelpText = "Access key secret")]
            public string SharedAccessKey { get; set; }
            [Option('p', "path", Required = true, HelpText = "Where to look for logfiles")]
            public string LogPath { get; set; }
        }
        static void Main(string[] args)
        {
            ITalkToLogAnalytics logClient = null;
            IWatchForLogs logWatcher = null;
            Parser.Default.ParseArguments<Options>(args)
            .WithParsed(o =>
            {
                var workspace = o.WorkspaceID ?? Environment.GetEnvironmentVariable("WORKSPACE_ID");
                var sharedKey = o.SharedAccessKey ?? Environment.GetEnvironmentVariable("WORKSPACE_SHARED_KEY");
                if (string.IsNullOrWhiteSpace(workspace))
                {
                    WriteError("Workspace ID missing - specify either by using -w or WORKSPACE_ID environment variable");
                    return;
                }

                if (string.IsNullOrEmpty(sharedKey))
                {
                    WriteError("Workspace shared access key missing - specify either by using -k or WORKSPACE_SHARED_KEY environment variable");
                    return;
                }

                logClient = new LogAnalyticsService(new System.Net.Http.HttpClient(), o.LogName, workspace, sharedKey);
                logWatcher = new LogWatcherService(o.LogPath);
            });
            if (logClient == null || logWatcher == null) return;
            logClient.OnError += (_, error) => WriteError(error);

            logWatcher.LogFileAdded += async (_, logFile) =>
            {
                Console.WriteLine($"-> {logFile}");
                try
                {
                    var json = await File.ReadAllTextAsync(logFile).ConfigureAwait(false);
                    var logData = JsonDocument.Parse(json).RootElement;
                    logClient.Log(logData);
                    File.Delete(logFile);
                }
                catch (Exception ex)
                {
                    WriteError(ex.Message);
                }
            };

            logWatcher.Start();
            while (true) { }
        }

        private static void WriteError(string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Failed to send log data: " + errorMessage);
            Console.ResetColor();
        }
    }
}
