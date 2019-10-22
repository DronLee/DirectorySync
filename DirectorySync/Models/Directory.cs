using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IO = System.IO;

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
        /// Конструктор.
        /// </summary>
        /// <param name="fullPath">Полный путо к директории.</param>
        /// <param name="itemFactory">Фабрика, отвечающая за создание элементов директории.</param>
        internal Directory(string fullPath, IItemFactory itemFactory)
        {
            FullPath = fullPath;
            var info = new IO.DirectoryInfo(fullPath);
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
        /// Последняя ошмбка, возникшая при загрузке директории.
        /// </summary>
        public string LastLoadError { get; private set; }

        /// <summary>
        /// Событие возникает при завершении загрузки директории.
        /// </summary>
        public event Action<IDirectory> LoadedDirectoryEvent;

        /// <summary>
        /// Событие возникает, после удаления директории и передаёт её.
        /// </summary>
        public event Action<IItem> DeletedEvent;

        /// <summary>
        /// Событие сообщает об ошибке, возникшей в процессе синхронизации.
        /// </summary>
        public event Action<string> SyncErrorEvent;

        /// <summary>
        /// Событие возникает, когда было выполнено копирование директории.
        /// Первый параметр - директория, созданная на основе копируемой.
        /// Второй параметр - путь, по которому осуществлялось копирование.
        /// </summary>
        public event Action<IItem, string> CopiedFromToEvent;

        /// <summary>
        /// Загрузка элементов директории.
        /// </summary>
        public async Task Load()
        {
            IsLoaded = false;
            _items.Clear();

            await LoadDirectories();
            await LoadFiles();

            if (LastLoadError == null)
                foreach (IDirectory directory in _items.Where(i => i is IDirectory))
                {
                    await directory.Load();
                    if(directory.LastLoadError != null && LastLoadError == null)
                        LastLoadError = "Есть директории, которые не удалось считать.";
                }

            IsLoaded = true;
            LoadedDirectoryEvent?.Invoke(this);
        }

        public async Task CopyTo(string destinationPath)
        {
            await Task.Run(async () =>
            {
                try
                {
                    IO.Directory.CreateDirectory(destinationPath);
                }
                catch { }

                foreach (var item in _items)
                    await item.CopyTo(IO.Path.Combine(destinationPath, item.Name));

                CopiedFromToEvent?.Invoke(_itemFactory.CreateDirectory(destinationPath), destinationPath);
            });
        }

        public async Task Delete()
        {
            await Task.Run(() =>
            {
                var error = false;
                try
                {
                    IO.Directory.Delete(FullPath, true);
                }
                catch
                {
                    SyncErrorEvent?.Invoke("Не удаётся удалить директорию: " + FullPath);
                    error = true;
                }
                if (!error)
                    DeletedEvent?.Invoke(this);
            });
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} {Name}";
        }

        private async Task LoadDirectories()
        {
            await Task.Run(() =>
            {
                string[] directories = null;
                try
                {
                    directories = IO.Directory.GetDirectories(FullPath);
                }
                catch { }
                if (directories == null)
                    LastLoadError = "Не удалось считать список папок директории: " + FullPath;
                else
                    foreach (var directoryPath in directories)
                        AddItem(_itemFactory.CreateDirectory(directoryPath));
            });
        }

        private async Task LoadFiles()
        {
            await Task.Run(() =>
            {
                string[] files = null;
                try
                {
                    files = IO.Directory.GetFiles(FullPath);
                }
                catch { }
                if (files == null)
                    LastLoadError = "Не удалось считать список файлов директории: " + FullPath;
                else
                    foreach (var filePath in files)
                        AddItem(_itemFactory.CreateFile(filePath));
            });
        }

        private void AddItem(IItem item)
        {
            item.DeletedEvent += (IItem deletedItem) => { _items.Remove(deletedItem); };
            _items.Add(item);
        }
    }
}