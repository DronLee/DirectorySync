﻿namespace DirectorySync.Models
{
    /// <summary>
    /// Интерфейс фабрики отслеживаемых элементов.
    /// </summary>
    public interface IItemFactory
    {
        /// <summary>
        /// Создание директории.
        /// </summary>
        /// <param name="directoryPath">Полный путь к директории.</param>
        /// <param name="excludedExtensions">Расширения файлов, которые не нужно считывать при загрузке директории.</param>
        /// <returns>Созданная директория.</returns>
        IDirectory CreateDirectory(string directoryPath, string[] excludedExtensions);

        /// <summary>
        /// Создание файла.
        /// </summary>
        /// <param name="filePath">Полный путь к файлу.</param>
        /// <returns>Созданный файл.</returns>
        IItem CreateFile(string filePath);
    }
}