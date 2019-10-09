using System;
using System.Threading.Tasks;

namespace DirectorySync.Models
{
    /// <summary>
    /// Интерфейс директории.
    /// </summary>
    public interface IDirectory : IItem
    {
        /// <summary>
        /// Коллекция элементов в директории.
        /// </summary>
        IItem[] Items { get; }

        /// <summary>
        /// Загрузка элементов директории.
        /// </summary>
        Task Load();

        /// <summary>
        /// Событие возникает при завершении загрузки директории.
        /// </summary>
        event Action<IDirectory> LoadedDirectoryEvent;

        /// <summary>
        /// True - директория загружена.
        /// </summary>
        bool IsLoaded { get; }
    }
}