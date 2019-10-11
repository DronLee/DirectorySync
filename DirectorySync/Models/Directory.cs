using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirectorySync.Models
{
    /// <summary>
    /// Директория.
    /// </summary>
    internal class Directory : IDirectory
    {
        private readonly IItemFactory _itemFactory;
        private readonly List<IItem> _items;

        /// <summary>
        /// Событие возникает при завершении загрузки директории.
        /// </summary>
        public event Action<IDirectory> LoadedDirectoryEvent;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="fullPath">Полный путо к директории.</param>
        /// <param name="itemFactory">Фабрика, отвечающая за создание элементов директории.</param>
        internal Directory(string fullPath, IItemFactory itemFactory)
        {
            FullPath = fullPath;
            var info = new System.IO.DirectoryInfo(fullPath);
            Name = info.Name;
            LastUpdate = info.LastWriteTime;

            _itemFactory = itemFactory;

            _items = new List<IItem>();
        }

        /// <summary>
        /// Коллекция элементов в директории.
        /// </summary>
        public IItem[] Items => _items.ToArray();

        /// <summary>
        /// Наименование директории.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Полный путь к директории.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Время последнего обновления директории.
        /// </summary>
        public DateTime LastUpdate { get; }

        /// <summary>
        /// True - директория загружена.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Событие возникает, после удаления элемента.
        /// </summary>
        public event Action DeletedEvent;

        /// <summary>
        /// Событие сообщает об ошибке, возникшей в процессе синхронизации.
        /// </summary>
        public event Action<string> SyncErrorEvent;

        /// <summary>
        /// Загрузка элементов директории.
        /// </summary>
        public async Task Load()
        {
            await Task.Run(() =>
            {
                foreach (var directoryPath in System.IO.Directory.GetDirectories(FullPath))
                    _items.Add(_itemFactory.CreateDirectory(directoryPath));
            });

            await Task.Run(() =>
            {
                foreach (var filePath in System.IO.Directory.GetFiles(FullPath))
                    _items.Add(_itemFactory.CreateFile(filePath));
            });

            foreach (IDirectory directory in _items.Where(i => i is IDirectory))
                await directory.Load();

            IsLoaded = true;
            LoadedDirectoryEvent?.Invoke(this);
        }

        public async Task CopyTo(string destinationPath)
        {
            await Task.Run(async () =>
            {
                try
                {
                    System.IO.Directory.CreateDirectory(destinationPath);
                }
                catch { }
                
                foreach (var item in _items)
                    await item.CopyTo(System.IO.Path.Combine(destinationPath, item.Name));
            });
        }

        public async Task Delete()
        {
            await Task.Run(() =>
            {
                var error = false;
                try
                {
                    System.IO.Directory.Delete(FullPath, true);
                }
                catch
                {
                    SyncErrorEvent?.Invoke("Не удаётся удалить директорию.");
                }
                if (!error)
                    DeletedEvent?.Invoke();
            });
        }
    }
}