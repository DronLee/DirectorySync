using System.ComponentModel;
using System.Windows.Input;

namespace DirectorySync.ViewModels
{
    public interface ISynchronizedItemViewModel : INotifyPropertyChanged
    { 
        string Name { get; }

        string IconPath { get; set; }

        ICommand AcceptCommand { get; }
    }
}