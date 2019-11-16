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
        /// Событие возникает, после удаления файла и передаёт его.
        /// </summary>
        public event Action<IItem> DeletedEvent;

        /// <summary>
        /// Событие сообщает об ошибке, возникшей в процессе синхронизации.
        /// </summary>
        public event Action<string> SyncErrorEvent;

        /// <summary>
        /// Событие возникает, когда было выполнено копирование файла.
        /// Первый параметр - файл, созданный на основе копируемого.
        /// Второй параметр - путь, по которому осуществлялось копирование.
        /// </summary>
        public event Action<IItem, string> CopiedFromToEvent;

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
                catch(Exception exc)
                {
                    SyncErrorEvent?.Invoke(exc.Message);
                }

                File destinationFile = null;
                try
                {
                    destinationFile = new File(destinationPath);
                }
                catch { }
                CopiedFromToEvent?.Invoke(destinationFile, destinationPath);
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
                catch(Exception exc)
                {
                    error = true;
                    SyncErrorEvent?.Invoke(exc.Message);
                }
                if (!error)
                    DeletedEvent?.Invoke(this);
            });
        }

        public override string ToString()
        {
            return $"{this.GetType().Name} {Name}";
        }
    }
}