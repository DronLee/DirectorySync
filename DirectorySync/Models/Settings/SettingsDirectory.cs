using Newtonsoft.Json;

namespace DirectorySync.Models.Settings
{
    /// <summary>
    /// Директория строки настройки.
    /// </summary>
    public class SettingsDirectory
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="directoryPath"></param>
        public SettingsDirectory(string directoryPath)
        {
            DirectoryPath = directoryPath;
        }

        /// <summary>
        /// Путь к директории.
        /// </summary>
        public string DirectoryPath { get; set; }

        /// <summary>
        /// True - директория не найдена.
        /// </summary>
        [JsonIgnore]
        public bool NotFound { get; set; }
    }
}