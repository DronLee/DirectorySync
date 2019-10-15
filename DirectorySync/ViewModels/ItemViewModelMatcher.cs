using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    public class ItemViewModelMatcher : IItemViewModelMatcher
    {
        public void UpdateStatusesAndCommands(IItemViewModel itemViewModel1, IItemViewModel itemViewModel2)
        {
            if (itemViewModel1.Item == null)
                OneItemIsMissing(itemViewModel1, itemViewModel2);
            else if (itemViewModel2.Item == null)
                OneItemIsMissing(itemViewModel2, itemViewModel1);
            else if (itemViewModel1.Item.LastUpdate > itemViewModel2.Item.LastUpdate)
                OneItemIsOlder(itemViewModel2, itemViewModel1);
            else if (itemViewModel1.Item.LastUpdate < itemViewModel2.Item.LastUpdate)
                OneItemIsOlder(itemViewModel1, itemViewModel2);
            else // Значит одинаковые
            {
                itemViewModel1.UpdateStatus(ItemStatusEnum.Equally);
                itemViewModel1.SetActionCommand(null);
                itemViewModel2.UpdateStatus(ItemStatusEnum.Equally);
                itemViewModel2.SetActionCommand(null);
            }
        }

        private void OneItemIsMissing(IItemViewModel missingItem, IItemViewModel thereIsItem)
        {
            missingItem.UpdateStatus(ItemStatusEnum.Missing);
            missingItem.SetActionCommand(() => thereIsItem.Item.Delete());

            thereIsItem.UpdateStatus(ItemStatusEnum.ThereIs);
            thereIsItem.SetActionCommand(() => thereIsItem.Item.CopyTo(missingItem.FullPath));
        }

        private void OneItemIsOlder(IItemViewModel olderItem, IItemViewModel newerItem)
        {
            olderItem.UpdateStatus(ItemStatusEnum.Older);
            olderItem.SetActionCommand(() => { olderItem.Item.CopyTo(newerItem.FullPath); });

            newerItem.UpdateStatus(ItemStatusEnum.Newer);
            newerItem.SetActionCommand(() => { newerItem.Item.CopyTo(olderItem.FullPath); });
        }
    }
}