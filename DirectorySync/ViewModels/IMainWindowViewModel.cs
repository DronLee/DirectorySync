using DirectorySync.Models;
using System.Threading.Tasks;

namespace DirectorySync.ViewModels
{
    public interface IMainWindowViewModel
    {
        IDirectory[] LeftDirectories { get; }

        IDirectory[] RightDirectories { get; }

        Task Load();
    }
}