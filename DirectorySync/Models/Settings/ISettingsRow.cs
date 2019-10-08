namespace DirectorySync.Models.Settings
{

    /// <summary>
    /// Интерфейс строки настроек.
    /// </summary>
    public interface ISettingsRow
    {
        /// <summary>
        /// Левая директория.
        /// </summary>
        SettingsDirectory LeftDirectory { get; set; }
        /// <summary>
        /// Правая директория.
        /// </summary>
        SettingsDirectory RightDirectory { get; set; }
        /// <summary>
        /// Директории строки отслеживаются.
        /// </summary>
        bool IsUsed { get; set; }
    }
}