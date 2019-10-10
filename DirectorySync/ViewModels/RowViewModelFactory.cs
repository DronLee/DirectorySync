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
                new ItemViewModel(synchronizedDirectories.RightDirectory), null);
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
        private RowViewModel[] CreateRowViewModels(IRowViewModel parentRow)
        {
            var leftDirectory = parentRow.LeftItem.Directory;
            var rightDirectory = parentRow.RightItem.Directory;

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
                        result.Add(LeftMissing(rightItem, leftDirectory.FullPath, parentRow));
                        break;
                    case -1:
                        leftItemIndex++;
                        result.Add(RightMissing(leftItem, rightDirectory.FullPath, parentRow));
                        break;
                    default:
                        leftItemIndex++;
                        rightItemIndex++;

                        if (leftItem is IDirectory && rightItem is IDirectory || !(leftItem is IDirectory) && !(rightItem is IDirectory))
                            result.Add(FullRow(leftItem, rightItem, parentRow));
                        else if (leftItem is IDirectory)
                        {
                            result.Add(RightMissing(leftItem, rightDirectory.FullPath, parentRow));
                            result.Add(LeftMissing(rightItem, leftDirectory.FullPath, parentRow));
                        }
                        else
                        {
                            result.Add(LeftMissing(rightItem, leftDirectory.FullPath, parentRow));
                            result.Add(RightMissing(leftItem, rightDirectory.FullPath, parentRow));
                        }
                        break;
                }
            }

            // Если с правой стороны элементов оказалось больше.
            for (; rightItemIndex < rightDirectoryItems.Length; rightItemIndex++)
                result.Add(LeftMissing(rightDirectoryItems[rightItemIndex], leftDirectory.FullPath, parentRow));

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
            var rightItemViewModel = new ItemViewModel(rightItem, ItemStatusEnum.ThereIs,
                () => rightItem.CopyTo(Path.Combine(leftItemDirectory, rightItem.Name)));
            var leftItemViewModel = new ItemViewModel(rightItem.Name, rightItemViewModel.IsDirectory, () => rightItem.Delete());
            var result = new RowViewModel(leftItemViewModel, rightItemViewModel, parentRow);
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
            var leftItemViewModel = new ItemViewModel(leftItem, ItemStatusEnum.ThereIs, 
                () => leftItem.CopyTo(Path.Combine(rightItemDirectory, leftItem.Name)));
            var rightItemViewModel = new ItemViewModel(leftItem.Name, leftItemViewModel.IsDirectory, () => leftItem.Delete());
            var result = new RowViewModel(leftItemViewModel, rightItemViewModel, parentRow);
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
            var leftItemViewModel = new ItemViewModel(leftItem);
            var rightItemViewModel = new ItemViewModel(rightItem);
            var result = new RowViewModel(leftItemViewModel, rightItemViewModel, parentRow);

            if (leftItem is IDirectory && rightItem is IDirectory &&
                (((IDirectory)leftItem).Items.Length > 0 || ((IDirectory)rightItem).Items.Length > 0))
            {
                foreach (var childItem in CreateRowViewModels(result))
                    result.ChildRows.Add(childItem);
                result.RefreshStatusesFromChilds();
            }
            else if (leftItem.LastUpdate > rightItem.LastUpdate)
            {
                leftItemViewModel.UpdateStatus(ItemStatusEnum.Newer);
                leftItemViewModel.SetActionCommand(() => { leftItem.CopyTo(rightItem.FullPath); });
                rightItemViewModel.UpdateStatus(ItemStatusEnum.Older);
                rightItemViewModel.SetActionCommand(() => { rightItem.CopyTo(leftItem.FullPath); });
            }
            else if (leftItem.LastUpdate < rightItem.LastUpdate)
            {
                leftItemViewModel.UpdateStatus(ItemStatusEnum.Older);
                leftItemViewModel.SetActionCommand(() => { leftItem.CopyTo(rightItem.FullPath); });
                rightItemViewModel.UpdateStatus(ItemStatusEnum.Newer);
                rightItemViewModel.SetActionCommand(() => { rightItem.CopyTo(leftItem.FullPath); });
            }
            else
            {
                leftItemViewModel.UpdateStatus(ItemStatusEnum.Equally);
                rightItemViewModel.UpdateStatus(ItemStatusEnum.Equally);
            }

            return result;
        }

        /// <summary>
        /// Обновление модели представления строки с созданием необходимых дочерних строк.
        /// </summary>
        /// <param name="rowViewModel">Обновляемая модель представления строки.</param>
        private void RefreshRow(IRowViewModel rowViewModel)
        {
            rowViewModel.RefreshChildRows(CreateRowViewModels(rowViewModel));
        }
    }
}