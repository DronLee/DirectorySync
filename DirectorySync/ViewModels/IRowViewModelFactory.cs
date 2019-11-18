using DirectorySync.Models;
using System;

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
        /// <param name="synchronizedItems">Пара синхронизируемых элементов, на основе которых строится строка.</param>
        /// <returns>Строка, отображающая отслеживаемые элементы.</returns>
        IRowViewModel CreateRowViewModel(ISynchronizedItems synchronizedItems);

        /// <summary>
        /// Событие добавления строки. Передаётся родительская строка и строка, которая добавляется.
        /// </summary>
        event Action<IRowViewModel, IRowViewModel> AddRowEvent;
    }
}