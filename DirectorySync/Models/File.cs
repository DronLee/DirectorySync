using System;
using System.Threading.Tasks;
using IO = System.IO;

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
            var info = new IO.FileInfo(fullPath);
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
        /// Событие сообщает об ошибке, возникшей в процессе синхронизации.
        /// </summary>
        public event Action<string> SyncErrorEvent;

        /// <summary>
        /// Событие возникает, когда было выполнено копирование файла.
        /// Первый параметр - копируемый файл.
        /// Второй параметр - файл, созданный на основе копируемого.
        /// </summary>
        public event Action<IItem, IItem> CopiedFromToEvent;

        /// <summary>
        /// Копировать элемент в указанный путь с заменой.
        /// </summary>
        /// <param name="destinationPath">Путь куда копировать.</param>
        public async Task CopyTo(string destinationPath)
        {
            await Task.Run(() =>
            {
                try
                {
                    IO.File.Copy(FullPath, destinationPath, true);
                }
                catch
                {
                    SyncErrorEvent?.Invoke("Не удаётся скопировать файл по пути: " + destinationPath);
                }

                File destinationFile = null;
                try
                {
                    destinationFile = new File(destinationPath);
                }
                catch { }
                CopiedFromToEvent?.Invoke(this, destinationFile);
            });
        }

        /// <summary>
        /// Удалить элемент.
        /// </summary>
        public async Task Delete()
        {
            await Task.Run(() =>
            {
                var error = false;
                try
                {
                    IO.File.Delete(FullPath);
                }
                catch
                {
                    error = true;
                    SyncErrorEvent?.Invoke("Не удаётся удалить файл.");
                }
                if (!error)
                    DeletedEvent?.Invoke();
            });
        }
    }
}