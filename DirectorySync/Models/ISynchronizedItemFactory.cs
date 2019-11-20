namespace DirectorySync.Models
{
    /// <summary>
    /// Интерфейс фабрики моделей синхронизируемых элементов.
    /// </summary>
    public interface ISynchronizedItemFactory : IItemFactory
    {
        /// <summary>
        /// Создание модели синхронизируемого файла.
        /// </summary>
        /// <param name="filePath">Полный путь к файлу.</param>
        /// <param name="file">Модель файла.</param>
        /// <returns>Модель синхронизируемого файла.</returns>
        ISynchronizedItem CreateSynchronizedFile(string filePath, IItem file);

        /// <summary>
        /// Создание модели синхронизируемой директории.
        /// </summary>
        /// <param name="directoryPath">Полный путь к директории.</param>
        /// <param name="directory">Модель директории.</param>
        /// <returns>Модель синхронизируемой директории.</returns>
        ISynchronizedItem CreateSynchronizedDirectory(string directoryPath, IDirectory directory);
    }
}