using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    /// <summary>
    /// Интерфейс фабрики создания моделей строк, отображающих отслеживаемые элементы.
    /// </summary>
    public interface IRowViewModelFactory
    {
        /// <summary>
        /// Создание строки, отображающей отслеживаемые элементы.
        /// </summary>
        /// <param name="synchronizedDirectories">Синхронизируемые директории.</param>
        /// <returns>Строка, отображающая отслеживаемые элементы.</returns>
        IRowViewModel CreateRowViewModel(ISynchronizedDirectories synchronizedDirectories);
    }
}