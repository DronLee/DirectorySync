using System.ComponentModel;

namespace DirectorySync.ViewModels.Settings
{
    public interface ISettingsDirectoryViewModel : INotifyPropertyChanged
    {
        string ButtonStyle { get; }
        string DirectoryPath { get; set; }
        bool NotFound { get; }
    }
}