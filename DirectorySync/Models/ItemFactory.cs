﻿namespace DirectorySync.Models
{
    /// <summary>
    /// Фабрика отслеживаемых элементов.
    /// </summary>
    public class ItemFactory : IItemFactory
    {
        /// <summary>
        /// Создание директории.
        /// </summary>
        /// <param name="directoryPath">Полный путь к директории.</param>
        /// <param name="excludedExtensions">Расширения файлов, которые не нужно считывать при загрузке директории.</param>
        /// <returns>Созданная директория. Если null, значит директорию не удалось найти.</returns>
        public IDirectory CreateDirectory(string directoryPath, string[] excludedExtensions)
        {
            try
            {
                return new Directory(directoryPath, excludedExtensions, this);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Создание файла.
        /// </summary>
        /// <param name="filePath">Полный путь к файлу.</param>
        /// <returns>Созданный файл. Если null значит файл не удалось найти.</returns>
        public IItem CreateFile(string filePath)
        {
            try
            {
                return new File(filePath);
            }
            catch
            {
                return null;
            }
        }
    }
}