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
        ISynchronizedDirectories[] SynchronizedDirectories { get; }

        /// <summary>
        /// Событие удаления одной из пары синхронизируемых директорий.
        /// </summary>
        event Action<ISynchronizedDirectories> RemoveSynchronizedDirectoriesEvent;

        /// <summary>
        /// Событие добавления пары синхронизируемых директорий.
        /// </summary>
        event Action<ISynchronizedDirectories> AddSynchronizedDirectoriesEvent;

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