using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    public class MainWindowViewModel : IMainWindowViewModel
    {
        private readonly List<ISynchronizedDirectories> _synchronizedDirectoriesList;
        private readonly IItemFactory _itemFactory; 

        public MainWindowViewModel(ISynchronizedDirectories[] synchronizedDirectoriesList, IItemFactory itemFactory)
        {
            _synchronizedDirectoriesList = synchronizedDirectoriesList.ToList();
            _itemFactory = itemFactory;
        }

        public IDirectory[] LeftDirectories { get; }

        public IDirectory[] RightDirectories { get; }

        public async Task Load()
        {
            foreach(var synchronizedDirectories in _synchronizedDirectoriesList)
                await synchronizedDirectories.Load();
        }
    }
}