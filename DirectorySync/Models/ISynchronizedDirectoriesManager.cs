using System.Threading.Tasks;

namespace DirectorySync.Models
{
    public interface ISynchronizedDirectoriesManager
    {
        IDirectory[] LeftDirectories { get; }

        IDirectory[] RightDirectories { get; }

        ISynchronizedDirectories[] SynchronizedDirectories { get; }

        Task Load();
    }
}