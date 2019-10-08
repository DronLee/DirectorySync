using System;
using System.ComponentModel;
using System.Windows.Input;

namespace DirectorySync.ViewModels.Settings
{
    /// <summary>
    /// Итерфейс модели представления строки настройки.
    /// </summary>
    public interface ISettingsRowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// True - пустая строка, которая нужна, чтобы пользователь мог добавить новые директории.
        /// </summary>
        bool IsEmpty { get; set; }

        /// <summary>
        /// Левая директория.
        /// </summary>
        ISettingsDirectoryViewModel LeftDirectory { get; set; }

        /// <summary>
        /// Правая директория.
        /// </summary>
        ISettingsDirectoryViewModel RightDirectory { get; set; }

        /// <summary>
        /// Директории строки отслеживаются.
        /// </summary>
        bool IsUsed { get; set; }

        /// <summary>
        /// Команда открытия диалога выбора директории.
        /// </summary>
        ICommand FolderDialogCommand { get; }

        /// <summary>
        /// Команда удаления строки.
        /// </summary>
        ICommand DeleteCommand { get; }

        /// <summary>
        /// Событие записи директории в пустую строку.
        /// </summary>
        event Action SetEmptyDirectoryEvent;

        /// <summary>
        /// Событие удаления строки.
        /// </summary>
        event Action<ISettingsRowViewModel> DeleteRowEvent;
    }
}