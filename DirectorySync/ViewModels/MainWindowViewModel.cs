using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    public class MainWindowViewModel : IMainWindowViewModel
    {
        private readonly ISynchronizedDirectoriesManager _synchronizedDirectoriesManager;

        public MainWindowViewModel(ISynchronizedDirectoriesManager synchronizedDirectoriesManager)
        {
            _synchronizedDirectoriesManager = synchronizedDirectoriesManager;
        }
    }
}