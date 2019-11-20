using System;
using System.IO;

namespace DirectorySync.Models
{
    /// <summary>
    /// Модель синхронизируемого элемента.
    /// </summary>
    public class SynchronizedItem : ISynchronizedItem
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="fullPath">Полный путь к элементу, описываемому моделью.</param>
        /// <param name="isDirectory">True - элемент является директорией.</param>
        /// <param name="item">Элемент, на основе которого строится директория.</param>
        public SynchronizedItem(string fullPath, bool isDirectory, IItem item)
        {
            (Name, FullPath, IsDirectory, Item) = (Path.GetFileName(fullPath), fullPath, isDirectory, item);

            if (item != null)
                UpdateItem(item);

            SyncCommand = new SyncCommand();
            SyncCommand.FinishedSyncEvent += () => { FinishedSyncEvent?.Invoke(this); };
        }

        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Полный руть к отслеживаемому элементу, который представляет данная модель.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// True - элемент является директорией.
        /// </summary>
        public bool IsDirectory { get; }

        /// <summary>
        /// Отображаемый моделью элемент синхронизации.
        /// </summary>
        public IItem Item { get; private set; }

        /// <summary>
        /// Отображаемая моделью директория. Если модель отображает файл, то null.
        /// </summary>
        public IDirectory Directory => Item as IDirectory;

        /// <summary>
        /// Статус элемента.
        /// </summary>
        public ItemStatus Status { get; private set; }

        /// <summary>
        /// Команда синхронизации.
        /// </summary>
        public SyncCommand SyncCommand { get; }

        /// <summary>
        /// Событие завершения синхронизации. Передаётся модель принятого элемента.
        /// </summary>
        public event Action<ISynchronizedItem> FinishedSyncEvent;

        /// <summary>
        /// Событие, сообщающее о завершении копирования.
        /// Передаёт синхронизируемый элемент, вызвавший копирвание, и элемент, созданный в реузльтате копирования.
        /// </summary>
        public event Action<ISynchronizedItem, IItem> CopiedFromToEvent;

        /// <summary>
        /// Событие возникновения ошибки в процессе синхронизации.
        /// </summary>
        public event Action<string> SyncErrorEvent;

        /// <summary>
        /// Событие изменения статуса.
        /// </summary>
        public event Action StatusChangedEvent;

        /// <summary>
        /// Обновление статуса.
        /// </summary>
        /// <param name="statusEnum">Новое значение статуса.</param>
        public void UpdateStatus(ItemStatusEnum statusEnum, string comment = null)
        {
            if (Status == null || Status.StatusEnum != statusEnum)
            {
                Status = new ItemStatus(statusEnum, comment);
                StatusChangedEvent?.Invoke();
            }
        }

        /// <summary>
        /// Обновление отслеживаемого элемента.
        /// </summary>
        /// <param name="item">Отслеживаемый элемент.</param>
        public void UpdateItem(IItem item)
        {
            if (Item != null)
            {
                // Больше не интересно за ним наблюдать.
                Item.SyncErrorEvent -= SyncError;
                Item.DeletedEvent -= Delete;
                Item.CopiedFromToEvent -= CopiedFromTo;
            }

            if (item != null)
            {
                item.SyncErrorEvent += SyncError;
                item.DeletedEvent += Delete;
                item.CopiedFromToEvent += CopiedFromTo;
            }

            Item = item;
        }

        private void SyncError(string error)
        {
            SyncErrorEvent?.Invoke(error);
        }

        private void Delete(IItem deletingItem)
        {
            UpdateItem(null);
        }

        private void CopiedFromTo(IItem newItem, string destinationPath)
        {
            CopiedFromToEvent?.Invoke(this, newItem);
        }
    }
}