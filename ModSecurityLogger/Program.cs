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
            [Option('l', "log", Required = false, HelpText = "Name of the datasource/log you wish to send data to")]
            public string LogName { get; set; }
            [Option('k', "key", Required = false, HelpText = "Access key secret")]
            public string SharedAccessKey { get; set; }
            [Option('p', "path", Required = false, HelpText = "Where to look for logfiles")]
            public string LogPath { get; set; }
            [Option('f', "config-file", Required = false, HelpText = "Provide all configuration via file")]
            public string ConfigFile { get; set; }
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
            Configuration config = new();
            Parser.Default.ParseArguments<Options>(args)
            .WithParsed(o =>
            {
                if (!string.IsNullOrEmpty(o.ConfigFile))
                {
                    config = Configuration.FromFile(o.ConfigFile);
                }
                var workspace = o.WorkspaceID ?? Environment.GetEnvironmentVariable("WORKSPACE_ID");
                if (!string.IsNullOrEmpty(workspace))
                {
                    config.WorkspaceId = workspace;
                }

                var sharedKey = o.SharedAccessKey ?? Environment.GetEnvironmentVariable("WORKSPACE_SHARED_KEY");
                if (!string.IsNullOrEmpty(sharedKey))
                {
                    config.SharedAccessKey = sharedKey;
                }

                if (!string.IsNullOrEmpty(o.LogName))
                {
                    config.LogName = o.LogName;
                }

                logClient = new LogAnalyticsService(new System.Net.Http.HttpClient(), config, loggerFactory.CreateLogger<ITalkToLogAnalytics>());
                log.LogInformation($"Watching {o.LogPath}...");

                if (IsRunningInContainer())
                {
                    log.LogInformation("Running in container - using polling method [10 seconds]");
                    logWatcher = new PollingLogWatcherService(config);
                }
                else
                {
                    logWatcher = new LogWatcherService(config);
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
                    log.LogInformation($"{logEntry.Transaction.UniqueId} - {logEntry.Transaction.ClientIp} -> {logEntry.Transaction.HostIp}:{logEntry.Transaction.HostPort}");
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
            while (true)
            {
                System.Threading.Thread.Sleep((int)TimeSpan.FromMinutes(5).TotalMilliseconds);
            }
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
