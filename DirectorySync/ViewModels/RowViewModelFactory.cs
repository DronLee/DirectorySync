using DirectorySync.Models;
using System;

namespace DirectorySync.ViewModels
{
    /// <summary>
    /// Фабрика создания моделей строк, отображающих отслеживаемые элементы.
    /// </summary>
    public class RowViewModelFactory : IRowViewModelFactory
    {
        /// <summary>
        /// Событие добавления строки. Передаётся родительская строка и строка, которая добавляется.
        /// </summary>
        public event Action<IRowViewModel, IRowViewModel> AddRowEvent;

        /// <summary>
        /// Событие удаления строки. Передаётся родительская строка и строка, которая удаляется.
        /// </summary>
        public event Action<IRowViewModel, IRowViewModel> DeleteRowEvent;

        /// <summary>
        /// Создание строки, отображающей отслеживаемые элементы.
        /// </summary>
        /// <param name="synchronizedItems">Пара синхронизируемых элементов, на основе которых строится строка.</param>
        /// <returns>Строка, отображающая отслеживаемые элементы.</returns>
        public IRowViewModel CreateRowViewModel(ISynchronizedItems synchronizedItems)
        {
            var result = new RowViewModel(new ItemViewModel(synchronizedItems.LeftItem),
                new ItemViewModel(synchronizedItems.RightItem), null);

            AddChildRows(result, synchronizedItems);

            synchronizedItems.DirectoriesIsLoadedEvent += (ISynchronizedItems loadedItems) =>
            {
                AddChildRows(result, loadedItems);
                result.LoadFinished();
            };

            return result;
        }

        private void AddChildRows(IRowViewModel row, ISynchronizedItems synchronizedItems)
        {
            foreach (var child in synchronizedItems.ChildItems)
            {
                var childRow = CreateRowViewModel(child);
                child.DeletedEvent += () => 
                { 
                    DeleteRowEvent?.Invoke(row, childRow); 
                };
                AddRowEvent?.Invoke(row, childRow);
            }
        }
    }
}