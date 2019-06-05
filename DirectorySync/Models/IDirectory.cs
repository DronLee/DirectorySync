namespace DirectorySync.Models
{
    /// <summary>
    /// Директория.
    /// </summary>
    internal interface IDirectory : IItem
    {
        /// <summary>
        /// Коллекция элементов в директории.
        /// </summary>
        IItem[] Items { get; }
    }
}