using System.Threading.Tasks;

namespace DirectorySync.Models
{
    public class SynchronizedDirectories : ISynchronizedDirectories
    {
        public SynchronizedDirectories(string leftDirectoryPath, string rightDirectoryPath, IItemFactory itemFactory)
        {
            LeftDirectory = itemFactory.CreateDirectory(leftDirectoryPath);
            RightDirectory = itemFactory.CreateDirectory(rightDirectoryPath);
        }

        public IDirectory LeftDirectory { get; }

        public IDirectory RightDirectory { get; }

        public bool IsLoaded => LeftDirectory.IsLoaded && RightDirectory.IsLoaded;

        public async Task Load()
        {
            await Task.Run(() => Task.WaitAll(LeftDirectory.Load(), RightDirectory.Load()));
        }
    }
}