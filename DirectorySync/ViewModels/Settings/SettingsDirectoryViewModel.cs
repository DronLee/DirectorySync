using System.ComponentModel;

namespace DirectorySync.ViewModels.Settings
{
    public class SettingsDirectoryViewModel : ISettingsDirectoryViewModel
    {
        private const string _emptyButtonStyleName = "EmptyDirectoryButton";

        public SettingsDirectoryViewModel()
        {
            ButtonStyle = _emptyButtonStyleName;
        }

        public SettingsDirectoryViewModel(string directoryPath, bool notFound, string buttonStyle)
        {
            DirectoryPath = directoryPath;
            NotFound = notFound;
            ButtonStyle = buttonStyle;
        }

        public string ButtonStyle { get; }

        public string DirectoryPath { get; set; }

        public bool NotFound { get; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}