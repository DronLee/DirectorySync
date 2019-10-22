using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    /// <summary>
    /// Класс для сравнения двух моделей представлений элементов и простановки в них статусов и команд синхронизации.
    /// </summary>
    public class ItemViewModelMatcher : IItemViewModelMatcher
    {
        /// <summary>
        /// Простановка в модели представления команд статусов и команд синхронизации.
        /// </summary>
        /// <param name="itemViewModel1">Модель представления одного элемента.</param>
        /// <param name="itemViewModel2">Модель представления второго элемента.</param>
        public void UpdateStatusesAndCommands(IItemViewModel itemViewModel1, IItemViewModel itemViewModel2)
        {
            if (itemViewModel1.Item == null)
                OneItemIsMissing(itemViewModel1, itemViewModel2);
            else if (itemViewModel2.Item == null)
                OneItemIsMissing(itemViewModel2, itemViewModel1);
            else
            {
                string loadError = null;
                if ((loadError = (itemViewModel1.Item as IDirectory)?.LastLoadError) != null ||
                    (loadError = (itemViewModel2.Item as IDirectory)?.LastLoadError) != null)
                {
                    itemViewModel1.UpdateStatus(ItemStatusEnum.LoadError, loadError);
                    itemViewModel2.UpdateStatus(ItemStatusEnum.LoadError, loadError);
                    itemViewModel1.SetActionCommand(null);
                    itemViewModel2.SetActionCommand(null);
                }
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
            olderItem.SetActionCommand(() => { return olderItem.Item.CopyTo(newerItem.FullPath); });

            newerItem.UpdateStatus(ItemStatusEnum.Newer);
            newerItem.SetActionCommand(() => { return newerItem.Item.CopyTo(olderItem.FullPath); });
        }
    }
}