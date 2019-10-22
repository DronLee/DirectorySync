using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    /// <summary>
    /// Фабрика создания моделей строк, отображающих отслеживаемые элементы.
    /// </summary>
    public class RowViewModelFactory : IRowViewModelFactory
    {
        private readonly IItemViewModelMatcher _itemViewModelMatcher;

        public RowViewModelFactory(IItemViewModelMatcher itemViewModelMatcher)
        {
            _itemViewModelMatcher = itemViewModelMatcher;
        }

        /// <summary>
        /// Событие сообщает о том, что строка была обновлена.
        /// </summary>
        public event Action<IRowViewModel> RefreshedRowEvent;

        /// <summary>
        /// Создание строки, отображающей отслеживаемые элементы.
        /// </summary>
        /// <param name="synchronizedDirectories">Синхронизируемые директории.</param>
        /// <returns>Строка, отображающая отслеживаемые элементы.</returns>
        public IRowViewModel CreateRowViewModel(ISynchronizedDirectories synchronizedDirectories)
        {
            var result = new RowViewModel(new ItemViewModel(synchronizedDirectories.LeftDirectory.FullPath, true, synchronizedDirectories.LeftDirectory),
                new ItemViewModel(synchronizedDirectories.RightDirectory.FullPath, true, synchronizedDirectories.RightDirectory), null);
            result.RowViewModelIsLoadedEvent += RefreshRow;
            return result;
        }

        /// <summary>
        /// Создание дочерних строк моделей представления отслеживаемых элементов для указанной строки.
        /// </summary>
        /// <param name="parentRow">Строка для которой создаются дочерние элементы.</param
        /// <returns>Строки моделей представления отслеживаемых элементов.</returns>
        private RowViewModel[] CreateRowViewModels(IRowViewModel parentRow)
        {
            var leftDirectory = parentRow.LeftItem.Directory;
            var rightDirectory = parentRow.RightItem.Directory;

            var leftDirectoryItems = leftDirectory.Items.OrderBy(i => i.ToString()).ToArray();
            var rightDirectoryItems = rightDirectory.Items.OrderBy(i => i.ToString()).ToArray();

            var result = new List<RowViewModel>(CreateRowViewModels(parentRow,
                leftDirectory.Items.Where(i => i is IDirectory).ToArray(), rightDirectory.Items.Where(i => i is IDirectory).ToArray()));
            result.AddRange(CreateRowViewModels(parentRow,
                leftDirectory.Items.Where(i => !(i is IDirectory)).ToArray(), rightDirectory.Items.Where(i => !(i is IDirectory)).ToArray()));

            return result.ToArray();
        }

        /// <summary>
        /// Создание строк моделей представления отслеживаемых элементов
        /// на основании коллекции отслеживаемых элементов слева и коллекции отслеживаемых элементов справа.
        /// </summary>
        /// <param name="parentRow">Строка для которой создаются дочерние элементы.</param>
        /// <param name="leftItems">Коллекция отслеживаемых элементов слева.</param>
        /// <param name="rightItems">Коллекция отслеживаемых элементов справа.</param>
        /// <returns>Строки моделей представления отслеживаемых элементов.</returns>
        private RowViewModel[] CreateRowViewModels(IRowViewModel parentRow, IItem[] leftItems, IItem[] rightItems)
        {
            var leftDirectory = parentRow.LeftItem.Directory;
            var rightDirectory = parentRow.RightItem.Directory;

            var result = new List<RowViewModel>();
            int rightItemIndex = 0;
            for (int leftItemIndex = 0; leftItemIndex < leftItems.Length;)
            {
                var leftItem = leftItems[leftItemIndex];

                // Может быть такое, что количество элементов слева больше,
                // тогда будут создваться записи с отсутсвующими справа элементами. 
                var rightItem = rightItemIndex < rightItems.Length ?
                    rightItems[rightItemIndex] : null;

                switch (rightItem == null ? -1 : leftItem.Name.CompareTo(rightItem.Name))
                {
                    case 1:
                        rightItemIndex++;
                        result.Add(LeftMissing(rightItem, leftDirectory.FullPath, parentRow));
                        break;
                    case -1:
                        leftItemIndex++;
                        result.Add(RightMissing(leftItem, rightDirectory.FullPath, parentRow));
                        break;
                    default:
                        leftItemIndex++;
                        rightItemIndex++;
                        result.Add(FullRow(leftItem, rightItem, parentRow));
                        break;
                }
            }

            // Если с правой стороны элементов оказалось больше.
            for (; rightItemIndex < rightItems.Length; rightItemIndex++)
                result.Add(LeftMissing(rightItems[rightItemIndex], leftDirectory.FullPath, parentRow));

            return result.ToArray();
        }

        /// <summary>
        /// Создание строки представления отслеживаемых элементов, в которой не хватает элемента слева.
        /// </summary>
        /// <param name="rightItem">Отслеживаемый правый элемент.</param>
        /// <param name="leftItemDirectory">Путь к директории слева. Нужен, чтобы задать команду удаления.</param>
        /// <param name="parentRow">Строка, куда войдёт создаваемая строка.</param>
        /// <returns>Строка представления отслеживаемых элементов.</returns>
        private RowViewModel LeftMissing(IItem rightItem, string leftItemDirectory, IRowViewModel parentRow)
        {
            var rightItemViewModel = new ItemViewModel(rightItem.FullPath, rightItem is IDirectory, rightItem);
            var leftItemViewModel = new ItemViewModel(Path.Combine(leftItemDirectory, rightItem.Name), rightItemViewModel.IsDirectory, null);

            _itemViewModelMatcher.UpdateStatusesAndCommands(leftItemViewModel, rightItemViewModel);

            var result = new RowViewModel(leftItemViewModel, rightItemViewModel, parentRow);
            result.RowViewModelIsLoadedEvent += RefreshRow;
            return result;
        }

        /// <summary>
        /// Создание строки представления отслеживаемых элементов, в которой не хватает элемента справа.
        /// </summary>
        /// <param name="leftItem">Отслеживаемый левый элемент.</param>
        /// <param name="rightItemDirectory">Путь к директории справа. Нужен, чтобы задать команду удаления.</param>
        /// <param name="parentRow">Строка, куда войдёт создаваемая строка.</param>
        /// <returns>Строка представления отслеживаемых элементов.</returns>
        private RowViewModel RightMissing(IItem leftItem, string rightItemDirectory, IRowViewModel parentRow)
        {
            var leftItemViewModel = new ItemViewModel(leftItem.FullPath, leftItem is IDirectory, leftItem);
            var rightItemViewModel = new ItemViewModel(Path.Combine(rightItemDirectory, leftItem.Name), leftItemViewModel.IsDirectory, null);

            _itemViewModelMatcher.UpdateStatusesAndCommands(leftItemViewModel, rightItemViewModel);

            var result = new RowViewModel(leftItemViewModel, rightItemViewModel, parentRow);
            result.RowViewModelIsLoadedEvent += RefreshRow;
            return result;
        }

        /// <summary>
        /// Создание полной строки представления отслеживаемых элементов, где есть и левый и правый элемент.
        /// </summary>
        /// <param name="leftItem">Отслеживаемый левый элемент.</param>
        /// <param name="rightItem">Отслеживаемый правый элемент.</param>
        /// <param name="parentRow">Строка, куда войдёт создаваемая строка.</param>
        /// <returns>Строка представления отслеживаемых элементов.</returns>
        private RowViewModel FullRow(IItem leftItem, IItem rightItem, IRowViewModel parentRow)
        {
            var leftItemViewModel = new ItemViewModel(leftItem.FullPath, leftItem is IDirectory, leftItem);
            var rightItemViewModel = new ItemViewModel(rightItem.FullPath, rightItem is IDirectory, rightItem);
            var result = new RowViewModel(leftItemViewModel, rightItemViewModel, parentRow);
            result.RowViewModelIsLoadedEvent += RefreshRow;

            if (leftItem is IDirectory && rightItem is IDirectory &&
                (((IDirectory)leftItem).Items.Length > 0 || ((IDirectory)rightItem).Items.Length > 0))
            {
                foreach (var childItem in CreateRowViewModels(result))
                    result.ChildRows.Add(childItem);
                result.RefreshStatusesFromChilds();
            }
            else
                _itemViewModelMatcher.UpdateStatusesAndCommands(leftItemViewModel, rightItemViewModel);

            return result;
        }

        /// <summary>
        /// Обновление модели представления строки с созданием необходимых дочерних строк.
        /// </summary>
        /// <param name="rowViewModel">Обновляемая модель представления строки.</param>
        private void RefreshRow(IRowViewModel rowViewModel)
        {
            if (rowViewModel.LeftItem.Directory != null && rowViewModel.RightItem.Directory != null)
            {
                rowViewModel.RefreshChildRows(CreateRowViewModels(rowViewModel));
                rowViewModel.RefreshStatusesFromChilds();
            }
            else
                _itemViewModelMatcher.UpdateStatusesAndCommands(rowViewModel.LeftItem, rowViewModel.RightItem);
            RefreshParentStatuses(rowViewModel);
            RefreshedRowEvent?.Invoke(rowViewModel);
        }

        /// <summary>
        /// Обновление статусов всех родительских строк.
        /// </summary>
        /// <param name="rowViewModel">Строка, у родителя которой будет обновлён статус.</param>
        private void RefreshParentStatuses(IRowViewModel rowViewModel)
        {
            if (rowViewModel.Parent != null)
            {
                rowViewModel.Parent.RefreshStatusesFromChilds();
                RefreshParentStatuses(rowViewModel.Parent);
            }
        }
    }
}