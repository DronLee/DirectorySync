using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace DirectorySync.ViewModels
{
    public interface IMainWindowViewModel : INotifyPropertyChanged
    {
        ObservableCollection<ISynchronizedItemsViewModel> SynchronizedItemsArray { get; }

        ICommand LoadDirectoriesCommand { get; }
    }
}