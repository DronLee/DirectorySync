using System.ComponentModel;
using System.Windows.Input;

namespace DirectorySync.ViewModels
{
    public interface IMainWindowViewModel : INotifyPropertyChanged
    {
        ISynchronizedDataRow[] SynchronizedDataRows { get; }

        ICommand LoadDirectoriesCommand { get; }
    }
}