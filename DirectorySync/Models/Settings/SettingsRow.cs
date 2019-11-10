using IODirectory = System.IO.Directory;

namespace DirectorySync.Models.Settings
{
    /// <summary>
    /// Строка настройки.
    /// </summary>
    public class SettingsRow : ISettingsRow
    {
        public SettingsRow() { }

        public SettingsRow(string leftDirectory, string rightDirectory, bool isUsed, string[] excludedExtensions)
        {
            LeftDirectory = new SettingsDirectory(leftDirectory);
            RightDirectory = new SettingsDirectory(rightDirectory);
            IsUsed = isUsed;
            ExcludedExtensions = excludedExtensions;
        }

        /// <summary>
        /// Левая директория.
        /// </summary>
        public SettingsDirectory LeftDirectory { get; set; }

        /// <summary>
        /// Правая директория.
        /// </summary>
        public SettingsDirectory RightDirectory { get; set; }

        /// <summary>
        /// Директории строки отслеживаются.
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// Расширения файлов, которые не должны принимать участие в синхронизации. 
        /// </summary>
        public string[] ExcludedExtensions { get; set; }

        /// <summary>
        /// Проверка существуют ли указанные директории и обновление свойства NotFound этих директорий.
        /// </summary>
        public void NotFoundRefresh()
        {
            LeftDirectory.NotFound = !IODirectory.Exists(LeftDirectory.DirectoryPath);
            RightDirectory.NotFound = !IODirectory.Exists(RightDirectory.DirectoryPath);
        }
    }
}