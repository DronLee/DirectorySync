namespace DirectorySync.Models
{
    /// <summary>
    /// Интерфейс фабрики моделей синхронизируемых элементов.
    /// </summary>
    public interface ISynchronizedItemFactory : IItemFactory
    {
        /// <summary>
        /// Создание синхронизируемого элемента.
        /// </summary>
        /// <param name="itemPath">Полный путь к отслеживаемому элементу.</param>
        /// <param name="isDirectory">True - отслеживаемый элемент яваляется директорией.</param>
        /// <param name="item">Отслеживаемый элемент. Может и отсутствовать.</param>
        /// <returns>Модель синхронизируемого элемента.</returns>
        ISynchronizedItem CreateSynchronizedItem(string itemPath, bool isDirectory, IItem item);

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