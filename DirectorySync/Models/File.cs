using System;
using System.Threading.Tasks;

namespace DirectorySync.Models
{
    /// <summary>
    /// Файл.
    /// </summary>
    internal class File : IItem
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="fullPath">Полный путь к файлу.</param>
        internal File(string fullPath)
        {
            FullPath = fullPath;
            var info = new System.IO.FileInfo(fullPath);
            Name = info.Name;
            LastUpdate = info.LastWriteTime;
        }

        /// <summary>
        /// Наименование файла.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Полный путь к файлу.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Время последнего обновления файла.
        /// </summary>
        public DateTime LastUpdate { get; }

        /// <summary>
        /// Событие возникает, после удаления элемента.
        /// </summary>
        public event Action DeletedEvent;

        /// <summary>
        /// Копировать элемент в указанный путь с заменой.
        /// </summary>
        /// <param name="destinationPath">Путь куда копировать.</param>
        public async Task CopyTo(string destinationPath)
        {
            await Task.Run(() =>
            {
                System.IO.File.Copy(FullPath, destinationPath, true);
            });
        }

        /// <summary>
        /// Удалить элемент.
        /// </summary>
        public async Task Delete()
        {
            await Task.Run(() =>
            {
                System.IO.File.Delete(FullPath);
                DeletedEvent?.Invoke();
            });
        }
    }
}