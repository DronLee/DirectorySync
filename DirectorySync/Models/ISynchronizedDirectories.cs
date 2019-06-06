using System.Threading.Tasks;

namespace DirectorySync.Models
{
    public interface ISynchronizedDirectories
    {
        IDirectory LeftDirectory { get; }

        IDirectory RightDirectory { get; }

        Task Load();
    }
}