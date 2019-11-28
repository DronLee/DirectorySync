﻿using System;
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
        private readonly ISynchronizedItemMatcher _synchronizedItemMatcher;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="settingsStorage">Хранилище настроек, где указаны директории для синхронизации.</param>
        /// <param name="synchronizedItemFactory">Фабрика для создания отслеживаемых элементов.</param>
        public SynchronizedDirectoriesManager(ISettingsStorage settingsStorage, ISynchronizedItemFactory synchronizedItemFactory, ISynchronizedItemMatcher synchronizedItemMatcher)
        {
            _settingsStorage = settingsStorage;
            _synchronizedItemFactory = synchronizedItemFactory;
            _synchronizedItemMatcher = synchronizedItemMatcher;
            _synchronizedDirectoriesList = settingsStorage.SettingsRows.Where(r => r.IsUsed).Select(
                r => new SynchronizedItems(r, _synchronizedItemFactory, _synchronizedItemMatcher)).ToList();
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

            // Обработка синхронизируемых директорий, которые указаны в настройках, но которых нет в данном менеджере.
            foreach (var settingsRow in activeSettingsRows.Where(r => !_synchronizedDirectoriesList.Any(d =>
                 d.LeftDirectory.FullPath == r.LeftDirectory.DirectoryPath && d.RightDirectory.FullPath == r.RightDirectory.DirectoryPath)))
            {
                var synchronizedDirectories = new SynchronizedItems(settingsRow, _synchronizedItemFactory, _synchronizedItemMatcher);
                _synchronizedDirectoriesList.Add(synchronizedDirectories);
                AddSynchronizedDirectoriesEvent?.Invoke(synchronizedDirectories);
            }

            // Обработка синхронизируемых директорий, которых уже нет в настройках, но которые ещё остались в менеджере.
            foreach (var synchronizedDirectory in _synchronizedDirectoriesList.Where(d => !activeSettingsRows.Any(r =>
                  d.LeftDirectory.FullPath == r.LeftDirectory.DirectoryPath && d.RightDirectory.FullPath == r.RightDirectory.DirectoryPath)).ToArray())
            {
                _synchronizedDirectoriesList.Remove(synchronizedDirectory);
                RemoveSynchronizedDirectoriesEvent?.Invoke(synchronizedDirectory);
            }

            // Если был изменён массив исключаемых из рассмотрения расширений файлов,
            // то должна выполняться перезагрузка синхронизируемых директорий с учётом этих изменений.
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
                    synchronizedDirectory.LoadRequired();
                }
            }

            // Все синхронизируемые директории, которые ещё не загружены, должны загрузиться.
            await Task.Run(() => Task.WhenAll(_synchronizedDirectoriesList.Where(d => !d.IsLoaded).Select(d => d.Load()).ToArray()));
        }
        
        /// <summary>
        /// Обновление содержимого синхронизируемых директорий.
        /// </summary>
        public async Task Refresh()
        {
            await Task.Run(() => Task.WhenAll(_synchronizedDirectoriesList.Select(d => d.Load()).ToArray()));
        }
    }
}