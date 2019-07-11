using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    public class ItemViewModelFactory : IItemViewModelFactory
    {
        public ISynchronizedDirectoryViewModel CreateDirectoryViewModel(IDirectory directory)
        {
            return new SynchronizedDirectoryViewModel(this, directory);
        }

        public ISynchronizedItemViewModel CreateItemViewModel(IItem file)
        {
            return new SynchronizedFileViewModel(file);
        }
    }
}