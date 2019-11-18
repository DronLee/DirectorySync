namespace DirectorySync.Models
{
    /// <summary>
    /// Класс для сравнения двух моделей синхронизируемых элементов и простановки в них статусов и команд синхронизации.
    /// </summary>
    public class SynchronizedItemMatcher : ISynchronizedItemMatcher
    {
        /// <summary>
        /// Простановка в модели синхронизируемых элементов статусов и команд синхронизации.
        /// </summary>
        /// <param name="item1">Модель одного элемента.</param>
        /// <param name="item2">Модель второго элемента.</param>
        public void UpdateStatusesAndCommands(ISynchronizedItem item1, ISynchronizedItem item2)
        {
            if (item1.Item == null)
                OneItemIsMissing(item1, item2);
            else if (item2.Item == null)
                OneItemIsMissing(item2, item1);
            else
            {
                string loadError = null;
                if ((loadError = (item1.Item as IDirectory)?.LastLoadError) != null ||
                    (loadError = (item2.Item as IDirectory)?.LastLoadError) != null)
                {
                    item1.UpdateStatus(ItemStatusEnum.LoadError, loadError);
                    item2.UpdateStatus(ItemStatusEnum.LoadError, loadError);
                    item1.SyncCommand.SetCommandAction(null);
                    item2.SyncCommand.SetCommandAction(null);
                }
                else if (item1.Item.LastUpdate > item2.Item.LastUpdate)
                    OneItemIsOlder(item2, item1);
                else if (item1.Item.LastUpdate < item2.Item.LastUpdate)
                    OneItemIsOlder(item1, item2);
                else // Значит одинаковые
                {
                    item1.UpdateStatus(ItemStatusEnum.Equally);
                    item1.SyncCommand.SetCommandAction(null);
                    item2.UpdateStatus(ItemStatusEnum.Equally);
                    item2.SyncCommand.SetCommandAction(null);
                }
            }
        }

        private void OneItemIsMissing(ISynchronizedItem missingItem, ISynchronizedItem thereIsItem)
        {
            missingItem.UpdateStatus(ItemStatusEnum.Missing);
            missingItem.SyncCommand.SetCommandAction(() => thereIsItem.Item.Delete());

            thereIsItem.UpdateStatus(ItemStatusEnum.ThereIs);
            thereIsItem.SyncCommand.SetCommandAction(() => thereIsItem.Item.CopyTo(missingItem.FullPath));
        }

        private void OneItemIsOlder(ISynchronizedItem olderItem, ISynchronizedItem newerItem)
        {
            olderItem.UpdateStatus(ItemStatusEnum.Older);
            olderItem.SyncCommand.SetCommandAction(() => { return olderItem.Item.CopyTo(newerItem.FullPath); });

            newerItem.UpdateStatus(ItemStatusEnum.Newer);
            newerItem.SyncCommand.SetCommandAction(() => { return newerItem.Item.CopyTo(olderItem.FullPath); });
        }
    }
}