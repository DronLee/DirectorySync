using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    public interface IMainWindowViewModel
    {
        IDirectory[] LeftDirectories { get; }

        IDirectory[] RightDirectories { get; }
    }
}