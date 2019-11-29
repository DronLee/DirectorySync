using DirectorySync.Models.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DirectorySync.Models
{
    /// <summary>
    /// Класс описывает пару синхронизируемых директорий.
    /// </summary>
    public class SynchronizedItems : ISynchronizedItems
    {
        private readonly ISettingsRow _settingsRow;
        private readonly ISynchronizedItemFactory _synchronizedItemFactory;
        private readonly ISynchronizedItemMatcher _synchronizedItemMatcher;
        private readonly ISynchronizedItemsStatusAndCommandsUpdater _statusAndCommandsUpdater;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="settingsRow">Строка настроек, соответствующая синхронизируемым элементам.</param>
        /// <param name="synchronizedItemFactory">Фабрика создания синхронизируемых элементов.</param>
        /// <param name="synchronizedItemMatcher">Объект, выполняющий сравнение синхронизируемых элементов между собой.</param>
        public SynchronizedItems(ISettingsRow settingsRow, ISynchronizedItemFactory synchronizedItemFactory, ISynchronizedItemMatcher synchronizedItemMatcher,
            ISynchronizedItemsStatusAndCommandsUpdater statusAndCommandsUpdater) :
            this(settingsRow, synchronizedItemFactory, synchronizedItemMatcher, statusAndCommandsUpdater,
                synchronizedItemFactory.CreateSynchronizedDirectory(settingsRow.LeftDirectory.DirectoryPath,
                    synchronizedItemFactory.CreateDirectory(settingsRow.LeftDirectory.DirectoryPath, settingsRow.ExcludedExtensions)),
                synchronizedItemFactory.CreateSynchronizedDirectory(settingsRow.RightDirectory.DirectoryPath,
                    synchronizedItemFactory.CreateDirectory(settingsRow.RightDirectory.DirectoryPath, settingsRow.ExcludedExtensions)))
        { }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="settingsRow">Строка настроек, соответствующая синхронизируемым элементам.</param>
        /// <param name="synchronizedItemFactory">Фабрика создания синхронизируемых элементов.</param>
        /// <param name="synchronizedItemMatcher">Объект, выполняющий сравнение синхронизируемых элементов между собой.</param>
        /// <param name="leftItem">Элемент синхронизации слева.</param>
        /// <param name="rightItem">Элемент синхронизации справва.</param>
        /// <param name="parentDirectories">Родительский элемент синхронизируемых директорий.</param>
        private SynchronizedItems(ISettingsRow settingsRow, ISynchronizedItemFactory synchronizedItemFactory, ISynchronizedItemMatcher synchronizedItemMatcher,
            ISynchronizedItemsStatusAndCommandsUpdater statusAndCommandsUpdater, ISynchronizedItem leftItem, ISynchronizedItem rightItem)
        {
            (_settingsRow, _synchronizedItemFactory, _synchronizedItemMatcher, _statusAndCommandsUpdater) =
                (settingsRow, synchronizedItemFactory, synchronizedItemMatcher, statusAndCommandsUpdater);
            (LeftItem, RightItem) = (leftItem, rightItem);

            if (LeftItem.Item != null)
                LeftItem.Item.DeletedEvent += ItemDeleted;
            if (RightItem.Item != null)
                RightItem.Item.DeletedEvent += ItemDeleted;

            LeftItem.FinishedSyncEvent += FinishedSync;
            RightItem.FinishedSyncEvent += FinishedSync;

            LeftItem.CopiedFromToEvent += CopiedFromItem;
            RightItem.CopiedFromToEvent += CopiedFromItem;
        }

        /// <summary>
        /// Левая директория.
        /// </summary>
        public IDirectory LeftDirectory => LeftItem.Item as IDirectory;

        /// <summary>
        /// Правая директория.
        /// </summary>
        public IDirectory RightDirectory => RightItem.Item as IDirectory;

        /// <summary>
        /// Левый элемент.
        /// </summary>
        public ISynchronizedItem LeftItem { get; private set; }

        /// <summary>
        /// Правый элемент.
        /// </summary>
        public ISynchronizedItem RightItem { get; private set; }

        /// <summary>
        /// Дочерние пары синхронизируемых элементов.
        /// </summary>
        public List<ISynchronizedItems> ChildItems { get; } = new List<ISynchronizedItems>();

        /// <summary>
        /// Событие начала загрузки отслеживаемых директорий.
        /// </summary>
        public event Action StartLoadDirectoriesEvent;

        /// <summary>
        /// Событие, возникающее при полной загрузке обоих директорий. Передаётся текущая модель.
        /// </summary>
        public event Action<ISynchronizedItems> DirectoriesIsLoadedEvent;

        /// <summary>
        /// Событие оповещает, что пара синхронизируемых элементов удаляется и передаёт запись на них.
        /// </summary>
        public event Action<ISynchronizedItems> DeleteEvent;

        /// <summary>
        /// Событие оповещает, что пара синхронизируемых элементов удалена.
        /// </summary>
        public event Action DeletedEvent;

        /// <summary>
        /// Загрузка директорий.
        /// </summary>
        public async Task Load()
        {
            StartLoadDirectoriesEvent?.Invoke();

            await Task.WhenAll(LeftDirectory.Load(), RightDirectory.Load());

            ClearChildItems();
            LoadChildItems();

            DirectoriesIsLoadedEvent?.Invoke(this);
        }

        /// <summary>
        /// Загрузка дочерних записей.
        /// </summary>
        public void LoadChildItems()
        {
            if (LeftDirectory != null && RightDirectory != null)
            {
                foreach (var directories in CreateChildItems(
                    LeftDirectory.Items.Where(i => i is IDirectory).ToArray(), RightDirectory.Items.Where(i => i is IDirectory).ToArray()))
                {
                    directories.LoadChildItems();
                    AddChildItem(directories);
                }

                foreach (var file in CreateChildItems(LeftDirectory.Items.Where(i => !(i is IDirectory)).ToArray(),
                    RightDirectory.Items.Where(i => !(i is IDirectory)).ToArray()))
                {
                    _synchronizedItemMatcher.UpdateStatusesAndCommands(file.LeftItem, file.RightItem);
                    AddChildItem(file);
                }

                RefreshLeftItemStatusAndCommands();
                RefreshRightItemStatusAndCommands();
            }
            else
                _synchronizedItemMatcher.UpdateStatusesAndCommands(LeftItem, RightItem);
        }

        /// <summary>
        /// Оповещение об удалении элемента.
        /// </summary>
        public void IsDeleted()
        {
            DeletedEvent?.Invoke();
        }

        private void AddChildItem(ISynchronizedItems child)
        {
            ChildItems.Add(child);
            child.DeleteEvent += DeleteChild;
            child.LeftItem.StatusChangedEvent += RefreshLeftItemStatusAndCommands;
            child.RightItem.StatusChangedEvent += RefreshRightItemStatusAndCommands;
        }

        private void RefreshLeftItemStatusAndCommands()
        {
            _statusAndCommandsUpdater.RefreshLeftItemStatusesAndCommandsFromChilds(this);
        }

        private void RefreshRightItemStatusAndCommands()
        {
            _statusAndCommandsUpdater.RefreshRightItemStatusesAndCommandsFromChilds(this);
        }

        private void DeleteChild(ISynchronizedItems deletingItems)
        {
            DeleteChildWithoutUpdateParent(deletingItems);
            RefreshLeftItemStatusAndCommands();
            RefreshRightItemStatusAndCommands();
        }

        private void DeleteChildWithoutUpdateParent(ISynchronizedItems child)
        {
            ChildItems.Remove(child);

            // Раз строка удаляется, больше за ней следить не надо.
            child.DeleteEvent -= DeleteChild;
            child.LeftItem.StatusChangedEvent -= RefreshLeftItemStatusAndCommands;
            child.RightItem.StatusChangedEvent -= RefreshRightItemStatusAndCommands;

            child.IsDeleted();
        }

        /// <summary>
        /// Создание дочерних записей.
        /// </summary>
        /// <param name="leftItems">Коллекция отслеживаемых элементов слева.</param>
        /// <param name="rightItems">Коллекция отслеживаемых элементов справа.</param>
        /// <returns>Строки синхронизируемых элементов.</returns>
        private ISynchronizedItems[] CreateChildItems(IItem[] leftItems, IItem[] rightItems)
        {
            var result = new List<ISynchronizedItems>();
            int rightItemIndex = 0;
            for (int leftItemIndex = 0; leftItemIndex < leftItems.Length;)
            {
                var leftItem = leftItems[leftItemIndex];

                // Может быть такое, что количество элементов слева больше,
                // тогда будут создваться записи с отсутсвующими справа элементами. 
                var rightItem = rightItemIndex < rightItems.Length ?
                    rightItems[rightItemIndex] : null;

                ISynchronizedItems childItem;

                switch (rightItem == null ? -1 : leftItem.Name.CompareTo(rightItem.Name))
                {
                    case 1:
                        rightItemIndex++;
                        childItem = LeftMissing(rightItem, LeftDirectory.FullPath);
                        break;
                    case -1:
                        leftItemIndex++;
                        childItem = RightMissing(leftItem, RightDirectory.FullPath);
                        break;
                    default:
                        leftItemIndex++;
                        rightItemIndex++;
                        childItem = CreateISynchronizedItems(_synchronizedItemFactory.CreateSynchronizedItem(leftItem.FullPath, leftItem is IDirectory, leftItem),
                            _synchronizedItemFactory.CreateSynchronizedItem(rightItem.FullPath, rightItem is IDirectory, rightItem));
                        break;
                }

                result.Add(childItem);
            }

            // Если с правой стороны элементов оказалось больше.
            for (; rightItemIndex < rightItems.Length; rightItemIndex++)
            {
                var childItem = LeftMissing(rightItems[rightItemIndex], LeftDirectory.FullPath);
                result.Add(childItem);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Создание синхронизируемых элементов без элемента слева.
        /// </summary>
        /// <param name="rightItem">Отслеживаемый правый элемент.</param>
        /// <param name="leftItemDirectory">Путь к директории расположения отслеживаемого элемента слева. Нужен, чтобы задать команду удаления.</param>
        /// <returns>Созданные инхронизируемые элементы.</returns>
        private ISynchronizedItems LeftMissing(IItem rightItem, string leftItemDirectory)
        {
            var rightSynchronizedItem = _synchronizedItemFactory.CreateSynchronizedItem(rightItem.FullPath, rightItem is IDirectory, rightItem);
            var leftSynchronizedItem = _synchronizedItemFactory.CreateSynchronizedItem(Path.Combine(leftItemDirectory, rightItem.Name), 
                rightSynchronizedItem.IsDirectory, null);

            return CreateISynchronizedItems(leftSynchronizedItem, rightSynchronizedItem);
        }

        /// <summary>
        /// Создание синхронизируемых элементов без элемента справа.
        /// </summary>
        /// <param name="leftItem">Отслеживаемый левый элемент.</param>
        /// <param name="rightItemDirectory">Путь к директории расположения отслеживаемого элемента справа. Нужен, чтобы задать команду удаления.</param>
        /// <returns>Созданные инхронизируемые элементы.</returns>
        private ISynchronizedItems RightMissing(IItem leftItem, string rightItemDirectory)
        {
            var leftSynchronizedItem = _synchronizedItemFactory.CreateSynchronizedItem(leftItem.FullPath, leftItem is IDirectory, leftItem);
            var rightSynchronizedItem = _synchronizedItemFactory.CreateSynchronizedItem(Path.Combine(rightItemDirectory, leftSynchronizedItem.Name), 
                leftSynchronizedItem.IsDirectory, null);

            return CreateISynchronizedItems(leftSynchronizedItem, rightSynchronizedItem);
        }

        private ISynchronizedItems CreateISynchronizedItems(ISynchronizedItem leftSynchronizedItem, ISynchronizedItem rightSynchronizedItem)
        {
            return new SynchronizedItems(_settingsRow, _synchronizedItemFactory, _synchronizedItemMatcher, _statusAndCommandsUpdater,
                leftSynchronizedItem, rightSynchronizedItem);
        }

        private void ItemDeleted(IItem item)
        {
            item.DeletedEvent -= ItemDeleted; // Раз удалился, значит больше не удалится.
            DeleteEvent?.Invoke(this);
        }

        private async void FinishedSync(ISynchronizedItem synchronizedItem)
        {
            var updatedItem = LeftItem == synchronizedItem ? RightItem : LeftItem;

            // Если Item == null, значит элемент удалили, и тут больше делать ничего не надо.
            if (updatedItem.Item != null)
            {
                StartLoadDirectoriesEvent?.Invoke();
                await updatedItem.Item.Load();

                // Была выполнена синхронизация, и нам не известно, обновлялись, удалялись или добавлялись дочерние элементы,
                // поэтому заново загрузим дочерние элементы удаляем все дочерние элементы и заново загружаем.
                ClearChildItems();
                LoadChildItems();

                DirectoriesIsLoadedEvent?.Invoke(this);
            }
        }

        private void ClearChildItems()
        {
            foreach (var child in ChildItems.ToArray())
                DeleteChildWithoutUpdateParent(child);
        }

        private void CopiedFromItem(ISynchronizedItem sourceSynchronizedItem, IItem newItem)
        {
            if (sourceSynchronizedItem == LeftItem)
                RightItem.UpdateItem(newItem);
            else
                LeftItem.UpdateItem(newItem);
        }
    }
}