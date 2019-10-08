namespace DirectorySync.ViewModels.Settings
{
    /// <summary>
    /// Интерфейс модели представления директории настроек.
    /// </summary>
    public interface ISettingsDirectoryViewModel
    {
        /// <summary>
        /// Наименование стиля кнопки выбора директории.
        /// </summary>
        string ButtonStyle { get; }

        /// <summary>
        /// Путь к директории.
        /// </summary>
        string DirectoryPath { get; set; }

        /// <summary>
        /// True - директория не найдена.
        /// </summary>
        bool NotFound { get; }
    }
}