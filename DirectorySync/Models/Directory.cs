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
        /// <param name="excludedExtensions">Расширения файлов, которые не нужно считывать.</param>
        /// <param name="itemFactory">Фабрика, отвечающая за создание элементов директории.</param>
        internal Directory(string fullPath, string[] excludedExtensions, IItemFactory itemFactory)
        {
            FullPath = fullPath;
            ExcludedExtensions = excludedExtensions;
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
        /// Расширения файлов, которые не нужно загружать.
        /// </summary>
        public string[] ExcludedExtensions { get; set; }

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

            _items.RemoveAll(i => (i as IDirectory)?.Items.Length == 0);

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

                CopiedFromToEvent?.Invoke(_itemFactory.CreateDirectory(destinationPath, ExcludedExtensions), destinationPath);
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
                catch(Exception exc)
                {
                    SyncErrorEvent?.Invoke(exc.Message);
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
                        AddItem(_itemFactory.CreateDirectory(directoryPath, ExcludedExtensions));
            });
        }

        private async Task LoadFiles()
        {
            await Task.Run(() =>
            {
                string[] files = null;
                try
                {
                    files = IO.Directory.GetFiles(FullPath).Where(f => ExcludedExtensions == null ||
                        !ExcludedExtensions.Contains(IO.Path.GetExtension(f).TrimStart('.'))).ToArray();
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