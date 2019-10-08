using System.Threading.Tasks;

namespace DirectorySync.Models
{
    public interface ISynchronizedDirectories
    {
        bool IsLoaded { get; }

        IDirectory LeftDirectory { get; }

        IDirectory RightDirectory { get; }

        Task Load();
    }
}