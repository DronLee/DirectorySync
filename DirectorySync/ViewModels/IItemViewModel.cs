using DirectorySync.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace DirectorySync.ViewModels
{
    public interface IItemViewModel : INotifyPropertyChanged
    { 
        string Name { get; }

        ItemStatus Status { get; set; }

        ICommand AcceptCommand { get; }

        ObservableCollection<IItemViewModel> Items { get; }

        void AddItems(IItemViewModel[] items);
    }
}