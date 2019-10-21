using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DirectorySync.ViewModels
{
    /// <summary>
    /// Интерфейс модели представления основного окна приложения.
    /// </summary>
    public interface IMainWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Gif для отображения процесса синхронизации.
        /// </summary>
        BitmapSource ProcessGifSource { get; }

        /// <summary>
        /// Строки, отображающие отслеживание директорий.
        /// </summary>
        ObservableCollection<IRowViewModel> Rows { get; }

        /// <summary>
        /// Строки лога.
        /// </summary>
        ObservableCollection<string> Log { get; }

        /// <summary>
        /// True - кнопка очистки лога видна.
        /// </summary>
        bool ClearLogButtonIsVisible { get; }

        /// <summary>
        /// Команда загрузки директорий.
        /// </summary>
        ICommand LoadedFormCommand { get; }

        /// <summary>
        /// Комнада выбора строки.
        /// </summary>
        ICommand SelectedItemCommand { get; }

        /// <summary>
        /// Команда вызова окна настроек.
        /// </summary>
        ICommand SettingsCommand { get; }

        /// <summary>
        /// Команда на очистку окна сообщений.
        /// </summary>
        ICommand ClearLogCommand { get; }
    }
}