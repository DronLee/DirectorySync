using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    public interface IItemViewModelFactory
    {
        event CreatedItems CreatedItemsEvent;

        IItemViewModel CreateItemViewModel(IItem item);

        IItemViewModel CreateMissingItemViewModel(string itemName);

        ISynchronizedItemsViewModel CreateSynchronizedDirectoriesViewModel(ISynchronizedDirectories synchronizedDirectories);
    }
}