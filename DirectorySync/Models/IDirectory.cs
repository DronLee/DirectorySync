using System.Threading.Tasks;

namespace DirectorySync.Models
{
    /// <summary>
    /// Директория.
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
        /// Директоиря была загружена.
        /// </summary>
        event LoadedDirectory LoadedDirectoryEvent;
    }
}