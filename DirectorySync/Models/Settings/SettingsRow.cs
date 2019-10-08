namespace DirectorySync.Models.Settings
{
    /// <summary>
    /// Строка настройки.
    /// </summary>
    public class SettingsRow : ISettingsRow
    {
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