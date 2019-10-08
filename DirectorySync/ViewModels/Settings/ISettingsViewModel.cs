using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace DirectorySync.ViewModels.Settings
{
    public interface ISettingsViewModel : INotifyPropertyChanged
    {
        MessageTypeEnum CommentType { get; }

        bool Ok { get; }

        ObservableCollection<ISettingsRowViewModel> SettingsRows { get; }

        ICommand OkCommand { get; }

        string Comment { get; set; }
    }
}