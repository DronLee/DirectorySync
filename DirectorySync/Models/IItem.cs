using System;

namespace DirectorySync.Models
{
    /// <summary>
    /// Элемент синхронизации.
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
        /// Дата последнего обновления элемента.
        /// </summary>
        DateTime LastUpdate { get; }
    }
}