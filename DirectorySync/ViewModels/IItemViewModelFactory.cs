using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    public interface IItemViewModelFactory
    {
        ISynchronizedDirectoryViewModel CreateDirectoryViewModel(IDirectory directory);

        ISynchronizedItemViewModel CreateItemViewModel(IItem file);
    }
}