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

        /// <summary>
        /// Проверка существуют ли указанные директории и обновление свойства NotFound этих директорий.
        /// </summary>
        void NotFoundRefresh();
    }
}