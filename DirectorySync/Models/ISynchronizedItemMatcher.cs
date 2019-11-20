namespace DirectorySync.Models
{
    /// <summary>
    /// Интерфейс класса для сравнения двух моделей синхронизируемых элементов и простановки в них статусов и команд синхронизации.
    /// </summary>
    public interface ISynchronizedItemMatcher
    {
        /// <summary>
        /// Простановка в модели синхронизируемых элементов статусов и команд синхронизации.
        /// </summary>
        /// <param name="item1">Модель одного элемента.</param>
        /// <param name="item2">Модель второго элемента.</param>
        void UpdateStatusesAndCommands(ISynchronizedItem item1, ISynchronizedItem item2);
    }
}