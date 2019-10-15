namespace DirectorySync.ViewModels
{
    /// <summary>
    /// Интерфейс класса для сравнения двух моделей представлений элементов и простановки в них статусов и команд синхронизации.
    /// </summary>
    public interface IItemViewModelMatcher
    {
        /// <summary>
        /// Простановка в модели представления команд статусов и команд синхронизации.
        /// </summary>
        /// <param name="itemViewModel1">Модель представления одного элемента.</param>
        /// <param name="itemViewModel2">Модель представления второго элемента.</param>
        void UpdateStatusesAndCommands(IItemViewModel itemViewModel1, IItemViewModel itemViewModel2);
    }
}