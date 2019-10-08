using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirectorySync.Models.Settings;

namespace DirectorySync.Models
{
    public class SynchronizedDirectoriesManager : ISynchronizedDirectoriesManager
    {
        private readonly ISettingsStorage _settingsStorage;
        private readonly IItemFactory _itemFactory;
        private readonly List<SynchronizedDirectories> _synchronizedDirectoriesList;

        public SynchronizedDirectoriesManager(ISettingsStorage settingsStorage, IItemFactory itemFactory)
        {
            _settingsStorage = settingsStorage;
            _itemFactory = itemFactory;
            _synchronizedDirectoriesList = settingsStorage.SettingsRows.Where(r => r.IsUsed).Select(
                r => new SynchronizedDirectories(r.LeftDirectory.DirectoryPath, r.RightDirectory.DirectoryPath, itemFactory)).ToList();
        }

        public IDirectory[] LeftDirectories => _synchronizedDirectoriesList.Select(d => d.LeftDirectory).ToArray();

        public IDirectory[] RightDirectories => _synchronizedDirectoriesList.Select(d => d.RightDirectory).ToArray();

        public ISynchronizedDirectories[] SynchronizedDirectories => _synchronizedDirectoriesList.ToArray();

        public event Action<ISynchronizedDirectories> RemoveSynchronizedDirectoryEvent;

        public async Task Load()
        {
            var activeSettingsRows = _settingsStorage.SettingsRows.Where(r => r.IsUsed).ToArray();

            // Обработка синхронизируемых директорий, которые указаны в настройках, но нет в данном менеджере.
            foreach (var settingsRow in activeSettingsRows.Where(r => !_synchronizedDirectoriesList.Any(d =>
                 d.LeftDirectory.FullPath == r.LeftDirectory.DirectoryPath && d.RightDirectory.FullPath == r.RightDirectory.DirectoryPath)))
                _synchronizedDirectoriesList.Add(new SynchronizedDirectories(settingsRow.LeftDirectory.DirectoryPath, settingsRow.RightDirectory.DirectoryPath, _itemFactory));

            // Обработка синхронизируемых директорий, которых уже нет в настройках, но которые ещё остались в менеджере.
            foreach (var synchronizedDirectory in _synchronizedDirectoriesList.Where(d => !activeSettingsRows.Any(r =>
                  d.LeftDirectory.FullPath == r.LeftDirectory.DirectoryPath && d.RightDirectory.FullPath == r.RightDirectory.DirectoryPath)).ToArray())
            {
                _synchronizedDirectoriesList.Remove(synchronizedDirectory);
                RemoveSynchronizedDirectoryEvent?.Invoke(synchronizedDirectory);
            }

            // Все синхронизируемые директории, которые ещё не загружены, должны загрузиться.
            foreach (var synchronizedDirectories in _synchronizedDirectoriesList.Where(d => !d.IsLoaded))
                await synchronizedDirectories.Load();
        }
    }
}