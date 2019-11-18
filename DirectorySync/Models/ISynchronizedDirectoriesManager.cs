using System;
using System.Threading.Tasks;

namespace DirectorySync.Models
{
    /// <summary>
    /// Интерфейс менеджера для работы с синхронизируемыми директориями.
    /// </summary>
    public interface ISynchronizedDirectoriesManager
    {
        /// <summary>
        /// Синхронизируемые директории, представленные по парам.
        /// </summary>
        ISynchronizedItems[] SynchronizedDirectories { get; }

        /// <summary>
        /// Событие удаления одной из пары синхронизируемых директорий.
        /// </summary>
        event Action<ISynchronizedItems> RemoveSynchronizedDirectoriesEvent;

        /// <summary>
        /// Событие добавления пары синхронизируемых директорий.
        /// </summary>
        event Action<ISynchronizedItems> AddSynchronizedDirectoriesEvent;

        /// <summary>
        /// Загрузка директорий.
        /// </summary>
        Task Load();

        /// <summary>
        /// Обновление содержимого синхронизируемых директорий.
        /// </summary>
        Task Refresh();
    }
}