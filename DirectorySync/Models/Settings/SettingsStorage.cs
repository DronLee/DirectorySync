using System.Text;
using Newtonsoft.Json;
using IO = System.IO;

namespace DirectorySync.Models.Settings
{
    /// <summary>
    /// Хранилище настроек.
    /// </summary>
    public class SettingsStorage : ISettingsStorage
    {
        private static readonly Encoding _encoding = Encoding.UTF8;
        
        private readonly string _settingsFile;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="settingsFile">Путь к файлу с настройками.</param>
        public SettingsStorage(string settingsFile)
        {
            _settingsFile = settingsFile;
            if (IO.File.Exists(_settingsFile))
            {
                var fileText = IO.File.ReadAllText(_settingsFile, _encoding);
                SettingsRows = JsonConvert.DeserializeObject<SettingsRow[]>(fileText);

                foreach (var row in SettingsRows)
                {
                    row.LeftDirectory.NotFound = !IO.Directory.Exists(row.LeftDirectory.DirectoryPath);
                    row.RightDirectory.NotFound = !IO.Directory.Exists(row.RightDirectory.DirectoryPath);
                }
            }
            else
                SettingsRows = new ISettingsRow[0];
        }

        /// <summary>
        /// Строки.
        /// </summary>
        public ISettingsRow[] SettingsRows { get; set; }

        /// <summary>
        /// Создание строки настройки.
        /// </summary>
        /// <param name="leftDirectoryPath">Путь к одной директории.</param>
        /// <param name="rightDirectoryPath">Путь ко второй директории.</param>
        /// <param name="isUsed">Признак активности настройки.</param>
        /// <returns>Созданная строка настроек.</returns>
        public ISettingsRow CreateSettingsRow(string leftDirectoryPath, string rightDirectoryPath, bool isUsed)
        {
            return new SettingsRow(leftDirectoryPath, rightDirectoryPath, isUsed);
        }

        /// <summary>
        /// Сохранение настроек в файл.
        /// </summary>
        public void Save()
        {
            IO.File.WriteAllText(_settingsFile,
                JsonConvert.SerializeObject(SettingsRows), _encoding);
        }
    }
}