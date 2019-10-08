using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace DirectorySync.ViewModels
{
    /// <summary>
    /// Интерфейс модели представления основного окна приложения.
    /// </summary>
    public interface IMainWindowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Строки, отображающие отслеживание директорий.
        /// </summary>
        ObservableCollection<IRowViewModel> Rows { get; }

        /// <summary>
        /// Команда загрузки директорий.
        /// </summary>
        ICommand LoadDirectoriesCommand { get; }

        /// <summary>
        /// Комнада выбора строки.
        /// </summary>
        ICommand SelectedItemCommand { get; }

        ICommand SettingsCommand { get; }
    }
}