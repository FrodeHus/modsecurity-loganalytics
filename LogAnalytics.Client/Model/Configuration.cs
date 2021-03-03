using System.IO;
using System.Text.Json;

namespace LogAnalytics.Client.Model
{
    public class Configuration
    {
        public Configuration()
        {

        }
        public Configuration(string workspaceId, string sharedAccessKey, string logName, string logDirectory)
        {
            WorkspaceId = workspaceId;
            SharedAccessKey = sharedAccessKey;
            LogName = logName;
            LogDirectory = logDirectory;
        }

        public static Configuration FromFile(string configurationFile)
        {
            var config = File.ReadAllText(configurationFile);
            return JsonSerializer.Deserialize<Configuration>(config, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public string WorkspaceId { get; set; }
        public string SharedAccessKey { get; set; }
        public string LogName { get; set; }
        public string LogDirectory { get; set; }
    }
}