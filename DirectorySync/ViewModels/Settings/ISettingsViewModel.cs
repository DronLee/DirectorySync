using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace DirectorySync.ViewModels.Settings
{
    /// <summary>
    /// Интерфейс модели представления настроек.
    /// </summary>
    public interface ISettingsViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Тип сообщения для пользователя в окне настроек.
        /// </summary>
        MessageTypeEnum CommentType { get; }

        /// <summary>
        /// True - окно закрыто с принятием настроек.
        /// </summary>
        bool Ok { get; }

        /// <summary>
        /// Модели представлений строк настроек.
        /// </summary>
        ObservableCollection<ISettingsRowViewModel> SettingsRows { get; }

        /// <summary>
        /// Команда принятия настроек.
        /// </summary>
        ICommand OkCommand { get; }

        /// <summary>
        /// Сообщение для пользователя в окне настроек.
        /// </summary>
        string Comment { get; set; }

        /// <summary>
        /// Актуализация коллекции строк.
        /// </summary>
        void RefreshRows();
    }
}