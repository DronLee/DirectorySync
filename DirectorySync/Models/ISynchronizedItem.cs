using System;

namespace DirectorySync.Models
{
    /// <summary>
    /// Интерфейс синхронизируемого элемента.
    /// </summary>
    public interface ISynchronizedItem
    {
        /// <summary>
        /// Наименование.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Полный руть к отслеживаемому элементу, который представляет данная модель.
        /// </summary>
        string FullPath { get; }

        /// <summary>
        /// True - элемент является директорией.
        /// </summary>
        bool IsDirectory { get; }

        /// <summary>
        /// Отображаемый моделью элемент синхронизации.
        /// </summary>
        IItem Item { get; }

        /// <summary>
        /// Отображаемая моделью директория. Если модель отображает файл, то null.
        /// </summary>
        IDirectory Directory { get; }

        /// <summary>
        /// Статус элемента.
        /// </summary>
        ItemStatus Status { get; }

        /// <summary>
        /// Команда синхронизации.
        /// </summary>
        SyncCommand SyncCommand { get; }

        /// <summary>
        /// Событие завершения синхронизации. Передаётся модель принятого элемента.
        /// </summary>
        event Action<ISynchronizedItem> FinishedSyncEvent;

        /// <summary>
        /// Событие, сообщающее о завершении копирования.
        /// Передаёт синхронизируемый элемент, вызвавший копирвание, и элемент, созданный в реузльтате копирования.
        /// </summary>
        event Action<ISynchronizedItem, IItem> CopiedFromToEvent;

        /// <summary>
        /// Событие возникновения ошибки в процессе синхронизации.
        /// </summary>
        event Action<string> SyncErrorEvent;

        /// <summary>
        /// Событие изменения статуса.
        /// </summary>
        event Action StatusChangedEvent;

        /// <summary>
        /// Обновление статуса.
        /// </summary>
        /// <param name="statusEnum">Новое значение статуса.</param>
        /// <param name="comment">Пояснение к статусу.</param>
        void UpdateStatus(ItemStatusEnum statusEnum, string comment = null);

        void UpdateItem(IItem item);
    }
}