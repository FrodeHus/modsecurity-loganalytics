using System;
using System.IO;
using CommandLine;
using LogAnalytics.Client.Service;
using System.Text.Json;
using LogAnalytics.Client.Model;
using Microsoft.Extensions.Logging;

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
            using ILoggerFactory loggerFactory =
               LoggerFactory.Create(builder =>
                   builder.AddSimpleConsole(options =>
                   {
                       options.IncludeScopes = true;
                       options.SingleLine = true;
                       options.TimestampFormat = "hh:mm:ss ";
                   }));

            var log = loggerFactory.CreateLogger<Program>();
            ITalkToLogAnalytics logClient = null;
            IWatchForLogs logWatcher = null;
            Parser.Default.ParseArguments<Options>(args)
            .WithParsed(o =>
            {
                var workspace = o.WorkspaceID ?? Environment.GetEnvironmentVariable("WORKSPACE_ID");
                var sharedKey = o.SharedAccessKey ?? Environment.GetEnvironmentVariable("WORKSPACE_SHARED_KEY");
                if (string.IsNullOrWhiteSpace(workspace))
                {
                    log.LogError("Workspace ID missing - specify either by using -w or WORKSPACE_ID environment variable");
                    return;
                }

                if (string.IsNullOrEmpty(sharedKey))
                {
                    log.LogError("Workspace shared access key missing - specify either by using -k or WORKSPACE_SHARED_KEY environment variable");
                    return;
                }

                logClient = new LogAnalyticsService(new System.Net.Http.HttpClient(), o.LogName, workspace, sharedKey, loggerFactory.CreateLogger<ITalkToLogAnalytics>());
                log.LogInformation($"Watching {o.LogPath}...");

                if (IsRunningInContainer())
                {
                    log.LogInformation("Running in container - using polling method [10 seconds]");
                    logWatcher = new PollingLogWatcherService(o.LogPath);
                }
                else
                {
                    logWatcher = new LogWatcherService(o.LogPath);
                }

            });
            if (logClient == null || logWatcher == null) return;
            logClient.OnError += (_, error) => WriteError(error);

            logWatcher.LogFileAdded += async (_, logFile) =>
            {
                try
                {
                    var json = await File.ReadAllTextAsync(logFile.FilePath).ConfigureAwait(false);
                    var logEntry = JsonSerializer.Deserialize<LogEntry>(json);
                    log.LogTrace($"{logEntry.Transaction.UniqueId} - {logEntry.Transaction.ClientIp} -> {logEntry.Transaction.HostIp}:{logEntry.Transaction.HostPort}");
                    logClient.Log(logEntry);
                    using var processed = File.Create(Path.Combine(logWatcher.GetProcessedFilesDirectory(), logFile.FileHash));
                    File.Delete(logFile.FilePath);
                }
                catch (Exception ex)
                {
                    log.LogError(ex.Message);
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

        private static bool IsRunningInContainer()
        {
            return bool.Parse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") ?? "false");
        }
    }
}
