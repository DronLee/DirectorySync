namespace DirectorySync.Models.Settings
{
    public interface ISettingsStorage
    {
        /// <summary>
        /// Строки.
        /// </summary>
        ISettingsRow[] SettingsRows { get; set; }

        /// <summary>
        /// Создание строки настройки.
        /// </summary>
        /// <param name="leftDirectoryPath">Путь к одной директории.</param>
        /// <param name="rightDirectoryPath">Путь ко второй директории.</param>
        /// <param name="isUsed">Признак активности настройки.</param>
        /// <returns>Созданная строка настроек.</returns>
        ISettingsRow CreateSettingsRow(string leftDirectoryPath, string rightDirectoryPath, bool isUsed);

        /// <summary>
        /// Сохранение настроек в файл.
        /// </summary>
        void Save();
    }
}