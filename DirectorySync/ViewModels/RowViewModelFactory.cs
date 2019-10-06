using System.Collections.Generic;
using System.IO;
using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    /// <summary>
    /// Фабрика создания моделей строк, отображающих отслеживаемые элементы.
    /// </summary>
    public class RowViewModelFactory : IRowViewModelFactory
    {
        /// <summary>
        /// Создание строки, отображающей отслеживаемые элементы.
        /// </summary>
        /// <param name="synchronizedDirectories">Синхронизируемые директории.</param>
        /// <returns>Строка, отображающая отслеживаемые элементы.</returns>
        public IRowViewModel CreateRowViewModel(ISynchronizedDirectories synchronizedDirectories)
        {
            var result = new RowViewModel(new ItemViewModel(synchronizedDirectories.LeftDirectory),
                new ItemViewModel(synchronizedDirectories.RightDirectory));
            result.RowViewModelIsLoadedEvent += RefreshRow;
            return result;
        }

        /// <summary>
        /// Создание строк представления отслеживаемых элементов
        /// на основании коллекции отслеживаемых элементов слева и коллекции отслеживаемых элементов справа.
        /// </summary>
        /// <param name="leftDirectoryItems">Коллекция отслеживаемых элментов слева.</param>
        /// <param name="rightDirectoryItems">Коллекция отслеживаемых элментов справа.</param>
        /// <returns>Строки представления отслеживаемых элементов.</returns>
        private RowViewModel[] CreateRowViewModels(IDirectory leftDirectory, IDirectory rightDirectory)
        {
            var leftDirectoryItems = leftDirectory.Items;
            var rightDirectoryItems = rightDirectory.Items;

            var result = new List<RowViewModel>();
            int rightItemIndex = 0;
            for (int leftItemIndex = 0; leftItemIndex < leftDirectoryItems.Length;)
            {
                var leftItem = leftDirectoryItems[leftItemIndex];

                // Может быть такое, что количество элементов слева больше,
                // тогда будут создваться записи с отсутсвующими справа элементами. 
                var rightItem = rightItemIndex < rightDirectoryItems.Length ?
                    rightDirectoryItems[rightItemIndex] : null;

                switch (rightItem == null ? -1 : leftItem.Name.CompareTo(rightItem.Name))
                {
                    case 1:
                        rightItemIndex++;
                        result.Add(LeftMissing(rightItem, leftDirectory.FullPath));
                        break;
                    case -1:
                        leftItemIndex++;
                        result.Add(RightMissing(leftItem, rightDirectory.FullPath));
                        break;
                    default:
                        leftItemIndex++;
                        rightItemIndex++;

                        if (leftItem is IDirectory && rightItem is IDirectory || !(leftItem is IDirectory) && !(rightItem is IDirectory))
                            result.Add(FullRow(leftItem, rightItem));
                        else if (leftItem is IDirectory)
                        {
                            result.Add(RightMissing(leftItem, rightDirectory.FullPath));
                            result.Add(LeftMissing(rightItem, leftDirectory.FullPath));
                        }
                        else
                        {
                            result.Add(LeftMissing(rightItem, leftDirectory.FullPath));
                            result.Add(RightMissing(leftItem, rightDirectory.FullPath));
                        }
                        break;
                }
            }

            // Если с правой стороны элементов оказалось больше.
            for (; rightItemIndex < rightDirectoryItems.Length; rightItemIndex++)
                result.Add(LeftMissing(rightDirectoryItems[rightItemIndex], leftDirectory.FullPath));

            return result.ToArray();
        }

        /// <summary>
        /// Создание строки представления отслеживаемых элементов, в которой не хватает элемента слева.
        /// </summary>
        /// <param name="rightItem">Отслеживаемый правый элемент.</param>
        /// <returns>Строка представления отслеживаемых элементов.</returns>
        private RowViewModel LeftMissing(IItem rightItem, string leftItemDirectory)
        {
            var rightItemViewModel = new ItemViewModel(rightItem, ItemStatusEnum.ThereIs,
                () => rightItem.CopyTo(Path.Combine(leftItemDirectory, rightItem.Name)));
            var leftItemViewModel = new ItemViewModel(rightItem.Name, rightItemViewModel.IsDirectory, () => rightItem.Delete());
            return new RowViewModel(leftItemViewModel, rightItemViewModel);
        }

        /// <summary>
        /// Создание строки представления отслеживаемых элементов, в которой не хватает элемента справа.
        /// </summary>
        /// <param name="leftItem">Отслеживаемый левый элемент.</param>
        /// <returns>Строка представления отслеживаемых элементов.</returns>
        private RowViewModel RightMissing(IItem leftItem, string rightItemDirectory)
        {
            var leftItemViewModel = new ItemViewModel(leftItem, ItemStatusEnum.ThereIs, 
                () => leftItem.CopyTo(Path.Combine(rightItemDirectory, leftItem.Name)));
            var rightItemViewModel = new ItemViewModel(leftItem.Name, leftItemViewModel.IsDirectory, () => leftItem.Delete());
            return new RowViewModel(leftItemViewModel, rightItemViewModel); ;
        }

        /// <summary>
        /// Создание полной строки представления отслеживаемых элементов, где есть и левый и правый элемент.
        /// </summary>
        /// <param name="leftItem">Отслеживаемый левый элемент.</param>
        /// <param name="rightItem">Отслеживаемый правый элемент.</param>
        /// <returns>Строка представления отслеживаемых элементов.</returns>
        private RowViewModel FullRow(IItem leftItem, IItem rightItem)
        {
            var leftItemViewModel = new ItemViewModel(leftItem);
            var rightItemViewModel = new ItemViewModel(rightItem);
            var result = new RowViewModel(leftItemViewModel, rightItemViewModel);

            if (leftItem is IDirectory && rightItem is IDirectory &&
                (((IDirectory)leftItem).Items.Length > 0 || ((IDirectory)rightItem).Items.Length > 0))
            {
                foreach (var childItem in CreateRowViewModels((IDirectory)leftItem, (IDirectory)rightItem))
                    result.ChildRows.Add(childItem);
                result.RefreshStatusesFromChilds();
            }
            else if (leftItem.LastUpdate > rightItem.LastUpdate)
            {
                leftItemViewModel.UpdateStatus(ItemStatusEnum.Newer);
                rightItemViewModel.UpdateStatus(ItemStatusEnum.Older);
            }
            else if (leftItem.LastUpdate < rightItem.LastUpdate)
            {
                leftItemViewModel.UpdateStatus(ItemStatusEnum.Older);
                rightItemViewModel.UpdateStatus(ItemStatusEnum.Newer);
            }
            else
            {
                leftItemViewModel.UpdateStatus(ItemStatusEnum.Equally);
                rightItemViewModel.UpdateStatus(ItemStatusEnum.Equally);
            }

            return result;
        }

        private void RefreshRow(IRowViewModel rowViewModel)
        {
            rowViewModel.RefreshChildRows(CreateRowViewModels(
                rowViewModel.LeftItem.Directory, rowViewModel.RightItem.Directory));
        }
    }
}