namespace DirectorySync.ViewModels.Settings
{
    /// <summary>
    /// Модель представления директории настроек.
    /// </summary>
    public class SettingsDirectoryViewModel : ISettingsDirectoryViewModel
    {
        private const string _emptyButtonStyleName = "EmptyDirectoryButton";

        /// <summary>
        /// Конструктор, создающий объект пустой директории.
        /// </summary>
        public SettingsDirectoryViewModel()
        {
            ButtonStyle = _emptyButtonStyleName;
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="directoryPath">Путь к директории.</param>
        /// <param name="notFound">True - директория не найдена.</param>
        /// <param name="buttonStyle">Наименование стиля кнопки выбора директории.</param>
        public SettingsDirectoryViewModel(string directoryPath, bool notFound, string buttonStyle)
        {
            DirectoryPath = directoryPath;
            NotFound = notFound;
            ButtonStyle = buttonStyle;
        }

        /// <summary>
        /// Наименование стиля кнопки выбора директории.
        /// </summary>
        public string ButtonStyle { get; }

        /// <summary>
        /// Путь к директории.
        /// </summary>
        public string DirectoryPath { get; set; }

        /// <summary>
        /// True - директория не найдена.
        /// </summary>
        public bool NotFound { get; }
    }
}