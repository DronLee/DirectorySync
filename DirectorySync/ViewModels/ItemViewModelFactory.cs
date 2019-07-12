using System.Linq;
using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    public class ItemViewModelFactory : IItemViewModelFactory
    {
        public event CreatedItems CreatedItemsEvent;

        public IItemViewModel CreateItemViewModel(IItem item)
        {
            var result = new ItemViewModel(item);
            if(item is IDirectory)
            {
                ((IDirectory)item).LoadedDirectoryEvent += LoadedDirectory;
                CreatedItemsEvent += result.AddItems;
            }
            return result;
        }

        private void LoadedDirectory(IDirectory directory)
        {
            CreatedItemsEvent?.Invoke(directory.Items.Select(i => new ItemViewModel(i)).ToArray());
        }

        public IItemViewModel CreateMissingItemViewModel(string itemName)
        {
            var result = new ItemViewModel(itemName); ;
            result.Status = new ItemStatus(ItemStatusEnum.Missing);
            return result;
        }

        public ISynchronizedItemsViewModel CreateSynchronizedDirectoriesViewModel(ISynchronizedDirectories synchronizedDirectories)
        {
            return new SynchronizedItemsViewModel(CreateItemViewModel(synchronizedDirectories.LeftDirectory),
                CreateItemViewModel(synchronizedDirectories.RightDirectory));
        }
    }
}