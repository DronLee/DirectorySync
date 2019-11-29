namespace DirectorySync.Models
{
    /// <summary>
    /// Фабрика моделей синхронизируемых элементов.
    /// </summary>
    public class SynchronizedItemFactory : ISynchronizedItemFactory
    {
        private readonly IItemFactory _itemFactory;

        public SynchronizedItemFactory(IItemFactory itemFactory)
        {
            _itemFactory = itemFactory;
        }

        /// <summary>
        /// Создание директории.
        /// </summary>
        /// <param name="directoryPath">Полный путь к директории.</param>
        /// <param name="excludedExtensions">Расширения файлов, которые не нужно считывать при загрузке директории.</param>
        /// <returns>Созданная директория.</returns>
        public IDirectory CreateDirectory(string directoryPath, string[] excludedExtensions)
        {
            return _itemFactory.CreateDirectory(directoryPath, excludedExtensions);
        }

        /// <summary>
        /// Создание файла.
        /// </summary>
        /// <param name="filePath">Полный путь к файлу.</param>
        /// <returns>Созданный файл.</returns>
        public IItem CreateFile(string filePath)
        {
            return _itemFactory.CreateFile(filePath);
        }

        /// <summary>
        /// Создание синхронизируемого элемента.
        /// </summary>
        /// <param name="itemPath">Полный путь к отслеживаемому элементу.</param>
        /// <param name="isDirectory">True - отслеживаемый элемент яваляется директорией.</param>
        /// <param name="item">Отслеживаемый элемент. Может и отсутствовать.</param>
        /// <returns>Модель синхронизируемого элемента.</returns>
        public ISynchronizedItem CreateSynchronizedItem(string itemPath, bool isDirectory, IItem item)
        {
            return isDirectory ? CreateSynchronizedDirectory(itemPath, item as IDirectory)
                : CreateSynchronizedFile(itemPath, item);
        }

        /// <summary>
        /// Создание модели синхронизируемой директории.
        /// </summary>
        /// <param name="directoryPath">Полный путь к директории.</param>
        /// <param name="directory">Модель директории.</param>
        /// <returns>Модель синхронизируемой директории.</returns>
        public ISynchronizedItem CreateSynchronizedDirectory(string directoryPath, IDirectory directory)
        {
            return new SynchronizedItem(directoryPath, true, directory);
        }

        /// <summary>
        /// Создание модели синхронизируемого файла.
        /// </summary>
        /// <param name="filePath">Полный путь к файлу.</param>
        /// <param name="file">Модель файла.</param>
        /// <returns>Модель синхронизируемого файла.</returns>
        public ISynchronizedItem CreateSynchronizedFile(string filePath, IItem file)
        {
            return new SynchronizedItem(filePath, false, file);
        }
    }
}