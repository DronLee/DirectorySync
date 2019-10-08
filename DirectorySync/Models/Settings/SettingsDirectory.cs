using Newtonsoft.Json;

namespace DirectorySync.Models.Settings
{
    public class SettingsDirectory
    {
        public SettingsDirectory(string directoryPath)
        {
            DirectoryPath = directoryPath;
        }

        public string DirectoryPath { get; set; }

        [JsonIgnore]
        public bool NotFound { get; set; }
    }
}