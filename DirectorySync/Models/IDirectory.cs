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
    }
}