namespace DirectorySync.Models
{
    /// <summary>
    /// Интерфейс класса, отвечающего за обновление статусов и комманд синхронизируемых элементов. 
    /// </summary>
    public interface ISynchronizedItemsStatusAndCommandsUpdater
    {
        /// <summary>
        /// Обновление статуса и команд левого элемента на основе дочерних.
        /// </summary>
        /// <param name="synchronizedItems">Синхронизируемые элементы, статусы и команды которых будут обновлены.</param>
        void RefreshLeftItemStatusesAndCommandsFromChilds(ISynchronizedItems synchronizedItems);

        /// <summary>
        /// Обновление статуса и команд правого элемента на основе дочерних.
        /// </summary>
        /// <param name="synchronizedItems">Синхронизируемые элементы, статусы и команды которых будут обновлены.</param>
        void RefreshRightItemStatusesAndCommandsFromChilds(ISynchronizedItems synchronizedItems);
    }
}