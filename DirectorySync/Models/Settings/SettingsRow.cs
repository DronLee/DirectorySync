namespace DirectorySync.Models.Settings
{
    /// <summary>
    /// Строка настройки.
    /// </summary>
    public class SettingsRow : ISettingsRow
    {
        public SettingsRow() { }

        public SettingsRow(string leftDirectory, string rightDirectory, bool isUsed)
        {
            LeftDirectory = new SettingsDirectory(leftDirectory);
            RightDirectory = new SettingsDirectory(rightDirectory);
            IsUsed = isUsed;
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
    }
}