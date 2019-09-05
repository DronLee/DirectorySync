using System.Collections.Generic;
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
        private RowViewModel[] CreateRowViewModels(IItem[] leftDirectoryItems, IItem[] rightDirectoryItems)
        {
            var result = new List<RowViewModel>();
            int rightItemIndex = 0;
            for (int leftItemIndex = 0; leftItemIndex < leftDirectoryItems.Length;)
            {
                var leftItem = leftDirectoryItems[leftItemIndex];
                var rightItem = rightDirectoryItems[rightItemIndex];

                switch (leftItem.Name.CompareTo(rightItem.Name))
                {
                    case 1:
                        rightItemIndex++;
                        result.Add(LeftMissing(rightItem));
                        break;
                    case -1:
                        leftItemIndex++;
                        result.Add(RightMissing(leftItem));
                        break;
                    default:
                        leftItemIndex++;
                        rightItemIndex++;

                        if (leftItem is IDirectory && rightItem is IDirectory || !(leftItem is IDirectory) && !(rightItem is IDirectory))
                            result.Add(FullRow(leftItem, rightItem));
                        else
                        {
                            result.Add(new RowViewModel(new ItemViewModel(leftItem, ItemStatusEnum.ThereIs), new ItemViewModel(leftItem.Name)));
                            result.Add(new RowViewModel(new ItemViewModel(rightItem.Name), new ItemViewModel(rightItem, ItemStatusEnum.ThereIs)));
                        }
                        break;
                }
            }

            // Если с правой стороны элементов оказалось больше.
            for (; rightItemIndex < rightDirectoryItems.Length; rightItemIndex++)
                result.Add(LeftMissing(rightDirectoryItems[rightItemIndex]));

            return result.ToArray();
        }

        /// <summary>
        /// Создание строки представления отслеживаемых элементов, в которой не хватает элемента слева.
        /// </summary>
        /// <param name="rightItem">Отслеживаемый правый элемент.</param>
        /// <returns>Строка представления отслеживаемых элементов.</returns>
        private RowViewModel LeftMissing(IItem rightItem)
        {
            return new RowViewModel(new ItemViewModel(rightItem.Name), new ItemViewModel(rightItem, ItemStatusEnum.ThereIs));
        }

        /// <summary>
        /// Создание строки представления отслеживаемых элементов, в которой не хватает элемента справа.
        /// </summary>
        /// <param name="leftItem">Отслеживаемый левый элемент.</param>
        /// <returns>Строка представления отслеживаемых элементов.</returns>
        private RowViewModel RightMissing(IItem leftItem)
        {
            return new RowViewModel(new ItemViewModel(leftItem, ItemStatusEnum.ThereIs), new ItemViewModel(leftItem.Name));
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

            if (leftItem is IDirectory && rightItem is IDirectory)
            {
                foreach (var childItem in CreateRowViewModels(((IDirectory)leftItem).Items, ((IDirectory)rightItem).Items))
                    result.ChildRows.Add(childItem);
                
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
                rowViewModel.LeftItem.Directory.Items, rowViewModel.RightItem.Directory.Items));
        }
    }
}