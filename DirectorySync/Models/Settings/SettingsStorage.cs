using System.Text;
using Newtonsoft.Json;
using IO = System.IO;

namespace DirectorySync.Models.Settings
{
    public class SettingsStorage : ISettingsStorage
    {
        private static readonly Encoding _encoding = Encoding.UTF8;
        
        private readonly string _settingsFile;

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

        public ISettingsRow[] SettingsRows { get; set; }

        public ISettingsRow CreateSettingsRow(string leftDirectoryPath, string rightDirectoryPath, bool isUsed)
        {
            return new SettingsRow { LeftDirectory = new SettingsDirectory(leftDirectoryPath), RightDirectory = new SettingsDirectory(rightDirectoryPath),
                IsUsed = isUsed };
        }

        public void Save()
        {
            IO.File.WriteAllText(_settingsFile,
                JsonConvert.SerializeObject(SettingsRows), _encoding);
        }
    }
}