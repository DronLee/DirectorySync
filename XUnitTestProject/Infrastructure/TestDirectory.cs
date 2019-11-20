using System;
using System.Collections.Generic;
using System.IO;

namespace XUnitTestProject.Infrastructure
{
    /// <summary>
    /// Класс описывает директорию, создаваемую для тестирования.
    /// </summary>
    internal class TestDirectory : IDisposable
    {
        public TestDirectory()
        {
            FullPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(FullPath);
        }

        public string FullPath { get;  }

        /// <summary>
        /// Создание пустых файлов в директории с проставлением даты последнего обновления в них.
        /// </summary>
        /// <param name="directoryPath">Путь к директории, в которой будут созданы файлы.</param>
        /// <param name="lastWriteTimesOnFileName">Словарь с перечислением дат последнего обновления файлов по их наименованиям.</param>
        public static void CreateFiles(string directoryPath, Dictionary<string, DateTime> lastWriteTimesOnFileName)
        {
            foreach (var lastWriteTimeOnFileName in lastWriteTimesOnFileName)
            {
                var filePath = Path.Combine(directoryPath, lastWriteTimeOnFileName.Key);
                File.WriteAllBytes(filePath, new byte[0]);
                File.SetLastWriteTime(filePath, lastWriteTimeOnFileName.Value);
            }
        }

        /// <summary>
        /// Создание вложенной директории.
        /// </summary>
        /// <param name="directoryPath">Директория, в которой нужно создать вложенную.</param>
        /// <param name="directoryName">Наименование вложенной директории.</param>
        /// <returns>Полный путь к созданной директории.</returns>
        public static string CreateDirectory(string directoryPath, string directoryName)
        {
            var result = Path.Combine(directoryPath, directoryName);
            Directory.CreateDirectory(result);
            return result;
        }

        /// <summary>
        /// Создание вложенной директории.
        /// </summary>
        /// <param name="directoryName">Наименование создаваемой директории.</param>
        /// <returns>Путь к созданной директории.</returns>
        public string CreateDirectory(string directoryName)
        {
            return CreateDirectory(FullPath, directoryName);
        }

        /// <summary>
        /// Создание вложенной директории с записью времени последнего обновления.
        /// </summary>
        /// <param name="directoryName">Наименование создаваемой директории.</param>
        /// <param name="lastWriteTime">Время последнего обновления.</param>
        /// <returns>Путь к созданной директории.</returns>
        public string CreateDirectory(string directoryName, DateTime lastWriteTime)
        {
            var result = CreateDirectory(directoryName);
            Directory.SetLastWriteTime(result, lastWriteTime);
            return result;
        }

        /// <summary>
        /// Создание пустых файлов в директории с проставлением даты последнего обновления в них.
        /// </summary>
        /// <param name="lastWriteTimesOnFileName">Словарь с перечислением дат последнего обновления файлов по их наименованиям.</param>
        public void CreateFiles(Dictionary<string, DateTime> lastWriteTimesOnFileName)
        {
            CreateFiles(FullPath, lastWriteTimesOnFileName);
        }

        /// <summary>
        /// Запись даты последнего обновления директории.
        /// </summary>
        /// <param name="lastWriteTime">Дата.</param>
        public void UpdateLastWriteTime(DateTime lastWriteTime)
        {
            Directory.SetLastWriteTime(FullPath, lastWriteTime);
        }

        /// <summary>
        /// Удаление директории.
        /// </summary>
        public void Dispose()
        {
            Directory.Delete(FullPath, true);
        }
    }
}