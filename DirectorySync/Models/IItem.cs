using System;
using System.Threading.Tasks;

namespace DirectorySync.Models
{
    /// <summary>
    /// Интерфейс отслеживаемого элемента.
    /// </summary>
    public interface IItem
    {
        /// <summary>
        /// Наименование элемента.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Полный путь к элементу на диске.
        /// </summary>
        string FullPath { get; }

        /// <summary>
        /// Время последнего обновления элемента.
        /// </summary>
        DateTime LastUpdate { get; }

        /// <summary>
        /// Событие возникает, после удаления элемента.
        /// </summary>
        event Action DeletedEvent;

        /// <summary>
        /// Событие возникает, когда было выполнено копирование элемента.
        /// Первый параметр - элемент, созданный на основе копируемого.
        /// Второй параметр - путь, по которому осуществлялось копирование.
        /// </summary>
        event Action<IItem, string> CopiedFromToEvent;

        /// <summary>
        /// Событие сообщает об ошибке, возникшей в процессе синхронизации.
        /// </summary>
        event Action<string> SyncErrorEvent;

        /// <summary>
        /// Копировать элемент в указанный путь с заменой.
        /// </summary>
        /// <param name="destinationPath">Путь куда копировать.</param>
        Task CopyTo(string destinationPath);

        /// <summary>
        /// Удалить элемент.
        /// </summary>
        Task Delete();
    }
}