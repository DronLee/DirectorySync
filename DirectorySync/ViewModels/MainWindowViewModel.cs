using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    public class MainWindowViewModel : IMainWindowViewModel
    {
        private readonly ISynchronizedDirectoriesManager _synchronizedDirectoriesManager;

        private ICommand _loadDirectoriesCommand;

        public MainWindowViewModel(ISynchronizedDirectoriesManager synchronizedDirectoriesManager, IItemViewModelFactory itemViewModelFactory)
        {
            _synchronizedDirectoriesManager = synchronizedDirectoriesManager;
            SynchronizedItemsArray = new ObservableCollection<ISynchronizedItemsViewModel>(_synchronizedDirectoriesManager.SynchronizedDirectories.Select(d =>
                itemViewModelFactory.CreateSynchronizedDirectoriesViewModel(d)));
        }

        public ICommand LoadDirectoriesCommand
        {
            get
            {
                if (_loadDirectoriesCommand == null)
                    _loadDirectoriesCommand = new Command(x => LoadDirectories());
                return _loadDirectoriesCommand;
            }
        }

        public ObservableCollection<ISynchronizedItemsViewModel> SynchronizedItemsArray { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void LoadDirectories()
        {
            await _synchronizedDirectoriesManager.Load();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SynchronizedItemsArray)));
        }
    }
}