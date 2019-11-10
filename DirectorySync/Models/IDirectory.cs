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
        /// True - директория загружена.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Последняя ошмбка возникшая при загрузке директории.
        /// </summary>
        string LastLoadError { get; }

        /// <summary>
        /// Расширения файлов, которые не нужно загружать.
        /// </summary>
        string[] ExcludedExtensions { get; set; }

        /// <summary>
        /// Событие возникает при завершении загрузки директории.
        /// </summary>
        event Action<IDirectory> LoadedDirectoryEvent;

        /// <summary>
        /// Загрузка элементов директории.
        /// </summary>
        Task Load();
    }
}