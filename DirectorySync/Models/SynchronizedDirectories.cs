using DirectorySync.Models.Settings;
using System.Threading.Tasks;

namespace DirectorySync.Models
{
    public class SynchronizedDirectories : ISynchronizedDirectories
    {
        public SynchronizedDirectories(ISettingsRow settingsRow, IItemFactory itemFactory)
        {
            LeftDirectory = itemFactory.CreateDirectory(settingsRow.LeftDirectory.DirectoryPath, settingsRow.ExcludedExtensions);
            RightDirectory = itemFactory.CreateDirectory(settingsRow.RightDirectory.DirectoryPath, settingsRow.ExcludedExtensions);
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