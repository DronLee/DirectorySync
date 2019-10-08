using System;
using System.ComponentModel;
using System.Windows.Input;

namespace DirectorySync.ViewModels.Settings
{
    public interface ISettingsRowViewModel : INotifyPropertyChanged
    {
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

        ICommand FolderDialogCommand { get; }

        ICommand DeleteCommand { get; }

        event Action SetEmptyDirectoryEvent;
        event Action<ISettingsRowViewModel> DeleteRowEvent;
    }
}