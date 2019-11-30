using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirectorySync.Models.Settings;

namespace DirectorySync.Models
{
    /// <summary>
    /// Менеджер для работы с синхронизируемыми директориями.
    /// </summary>
    public class SynchronizedDirectoriesManager : ISynchronizedDirectoriesManager
    {
        private readonly ISettingsStorage _settingsStorage;
        private readonly ISynchronizedItemFactory _synchronizedItemFactory;
        private readonly List<SynchronizedItems> _synchronizedDirectoriesList;
        private readonly ISynchronizedItemsStatusAndCommandsUpdater _synchronizedItemsStatusAndCommandsUpdater;

        /// <summary>
        /// Синхронизируемые директории, отобранные для загрузки.
        /// </summary>
        private readonly List<SynchronizedItems> _synchronizedDirectoriesListForLoad;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="settingsStorage">Хранилище настроек, где указаны директории для синхронизации.</param>
        /// <param name="synchronizedItemFactory">Фабрика для создания отслеживаемых элементов.</param>
        public SynchronizedDirectoriesManager(ISettingsStorage settingsStorage, ISynchronizedItemFactory synchronizedItemFactory,
            ISynchronizedItemsStatusAndCommandsUpdater synchronizedItemsStatusAndCommandsUpdater)
        {
            (_settingsStorage, _synchronizedItemFactory, _synchronizedItemsStatusAndCommandsUpdater) = 
                (settingsStorage, synchronizedItemFactory, synchronizedItemsStatusAndCommandsUpdater);

            _synchronizedDirectoriesList = settingsStorage.SettingsRows.Where(r => r.IsUsed).Select(
                r => new SynchronizedItems(r, _synchronizedItemFactory, _synchronizedItemsStatusAndCommandsUpdater)).ToList();
            _synchronizedDirectoriesListForLoad = new List<SynchronizedItems>(_synchronizedDirectoriesList);
        }

        /// <summary>
        /// Синхронизируемые директории, представленные по парам.
        /// </summary>
        public ISynchronizedItems[] SynchronizedDirectories => _synchronizedDirectoriesList.ToArray();

        /// <summary>
        /// Событие удаления одной из пары синхронизируемых директорий.
        /// </summary>
        public event Action<ISynchronizedItems> RemoveSynchronizedDirectoriesEvent;

        /// <summary>
        /// Событие добавления пары синхронизируемых директорий.
        /// </summary>
        public event Action<ISynchronizedItems> AddSynchronizedDirectoriesEvent;

        /// <summary>
        /// Загрузка директорий.
        /// </summary>
        public async Task Load()
        {
            var activeSettingsRows = _settingsStorage.SettingsRows.Where(r => r.IsUsed).ToArray();

            // Отбор синхронизируемых директорий, которые указаны в настройках, но которых ещё нет в данном менеджере.
            foreach (var settingsRow in activeSettingsRows.Where(r => !_synchronizedDirectoriesList.Any(d =>
                 d.LeftDirectory.FullPath == r.LeftDirectory.DirectoryPath && d.RightDirectory.FullPath == r.RightDirectory.DirectoryPath)))
            {
                var synchronizedDirectories = new SynchronizedItems(settingsRow, _synchronizedItemFactory,
                    _synchronizedItemsStatusAndCommandsUpdater);
                _synchronizedDirectoriesList.Add(synchronizedDirectories);
                AddSynchronizedDirectoriesEvent?.Invoke(synchronizedDirectories);

                _synchronizedDirectoriesListForLoad.Add(synchronizedDirectories);
            }

            // Удаление синхронизируемых директорий, которых уже нет в настройках, но которые ещё остались в менеджере.
            foreach (var synchronizedDirectory in _synchronizedDirectoriesList.Where(d => !activeSettingsRows.Any(r =>
                  d.LeftDirectory.FullPath == r.LeftDirectory.DirectoryPath && d.RightDirectory.FullPath == r.RightDirectory.DirectoryPath)).ToArray())
            {
                _synchronizedDirectoriesList.Remove(synchronizedDirectory);
                RemoveSynchronizedDirectoriesEvent?.Invoke(synchronizedDirectory);
            }

            _synchronizedDirectoriesListForLoad.AddRange(GetSynchronizedDirectoriesListWithChangedExcludedExtensions(activeSettingsRows));

            // Все синхронизируемые директории, которые ещё не загружены, должны загрузиться.
            await Load(_synchronizedDirectoriesListForLoad);

            _synchronizedDirectoriesListForLoad.Clear();
        }
        
        /// <summary>
        /// Обновление содержимого синхронизируемых директорий.
        /// </summary>
        public async Task Refresh()
        {
            await Load(_synchronizedDirectoriesList);
        }

        private async Task Load(List<SynchronizedItems> synchronizedDirectories)
        {
            await Task.Run(() => Task.WhenAll(synchronizedDirectories.Select(d => d.Load()).ToArray()));
        }

        /// <summary>
        /// Получение списка синхронизируемых директорий,
        /// для которых был изменён массив исключаемых из рассмотрения расширений файлов.
        /// </summary>
        /// <param name="activeSettingsRows">Активные строки настройки.</param>
        /// <returns>Полученный список синхронизируемых директорий.</returns>
        private IEnumerable<SynchronizedItems> GetSynchronizedDirectoriesListWithChangedExcludedExtensions(ISettingsRow[] activeSettingsRows)
        {
            var result = new List<SynchronizedItems>();

            foreach (var synchronizedDirectory in _synchronizedDirectoriesList)
            {
                var settingsRow = activeSettingsRows.Single(r =>
                    synchronizedDirectory.LeftDirectory.FullPath == r.LeftDirectory.DirectoryPath &&
                    synchronizedDirectory.RightDirectory.FullPath == r.RightDirectory.DirectoryPath);
                if (settingsRow.ExcludedExtensions != null &&

                    // Достаточно проверить директорию с одной стороны, так как массив ExcludedExtensions для обоих сторон одниковый.
                    !settingsRow.ExcludedExtensions.SequenceEqual(synchronizedDirectory.LeftDirectory.ExcludedExtensions) ||
                    settingsRow.ExcludedExtensions != null && synchronizedDirectory.LeftDirectory.ExcludedExtensions == null)
                {
                    synchronizedDirectory.LeftDirectory.ExcludedExtensions = settingsRow.ExcludedExtensions;
                    synchronizedDirectory.RightDirectory.ExcludedExtensions = settingsRow.ExcludedExtensions;

                    result.Add(synchronizedDirectory);
                }
            }

            return result;
        }
    }
}