namespace DirectorySync.Models.Settings
{
    public interface ISettingsStorage
    { 
        ISettingsRow[] SettingsRows { get; set; }

        ISettingsRow CreateSettingsRow(string leftDirectoryPath, string rightDirectoryPath, bool isUsed);

        void Save();
    }
}