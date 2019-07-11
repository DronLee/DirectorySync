using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    public class MainWindowViewModel : IMainWindowViewModel
    {
        private readonly ISynchronizedDirectoriesManager _synchronizedDirectoriesManager;

        private ICommand _loadDirectoriesCommand;

        public MainWindowViewModel(ISynchronizedDirectoriesManager synchronizedDirectoriesManager)
        {
            _synchronizedDirectoriesManager = synchronizedDirectoriesManager;
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

        public ISynchronizedDataRow[] SynchronizedDataRows => throw new NotImplementedException();

        ICommand IMainWindowViewModel.LoadDirectoriesCommand { get => throw new NotImplementedException(); }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void LoadDirectories()
        {
            await _synchronizedDirectoriesManager.Load();
        }
    }
}