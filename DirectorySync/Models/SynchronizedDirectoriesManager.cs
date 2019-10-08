using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirectorySync.Models.Settings;

namespace DirectorySync.Models
{
    public class SynchronizedDirectoriesManager : ISynchronizedDirectoriesManager
    {
        private readonly List<SynchronizedDirectories> _synchronizedDirectoriesList;

        public SynchronizedDirectoriesManager(ISettingsStorage settingsStorage, IItemFactory itemFactory)
        {
            _synchronizedDirectoriesList = settingsStorage.SettingsRows.Where(r => r.IsUsed).Select(
                r => new SynchronizedDirectories(r.LeftDirectory.DirectoryPath, r.RightDirectory.DirectoryPath, itemFactory)).ToList();
        }

        public IDirectory[] LeftDirectories => _synchronizedDirectoriesList.Select(d => d.LeftDirectory).ToArray();

        public IDirectory[] RightDirectories => _synchronizedDirectoriesList.Select(d => d.RightDirectory).ToArray();

        public ISynchronizedDirectories[] SynchronizedDirectories => _synchronizedDirectoriesList.ToArray();

        public async Task Load()
        {
            foreach (var synchronizedDirectories in _synchronizedDirectoriesList)
                await synchronizedDirectories.Load();
        }
    }
}