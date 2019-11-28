﻿using DirectorySync.Models.Settings;
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
        private static readonly Dictionary<ItemStatusEnum, string> _statusCommentsFromChildren = new Dictionary<ItemStatusEnum, string>
            {
                {ItemStatusEnum.Missing, "Не хватает тех элементов, что есть с другой стороны"},
                {ItemStatusEnum.ThereIs, "Содержит отсутствующие с другой стороны элементы"},
                {ItemStatusEnum.Older, "Содержит более старые"},
                {ItemStatusEnum.Newer, "Содержит более новые"}
            };

        private readonly ISettingsRow _settingsRow;
        private readonly ISynchronizedItemFactory _synchronizedItemFactory;
        private readonly ISynchronizedItemMatcher _synchronizedItemMatcher;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="settingsRow">Строка настроек, соответствующая синхронизируемым элементам.</param>
        /// <param name="synchronizedItemFactory">Фабрика создания синхронизируемых элементов.</param>
        /// <param name="synchronizedItemMatcher">Объект, выполняющий сравнение синхронизируемых элементов между собой.</param>
        public SynchronizedItems(ISettingsRow settingsRow, ISynchronizedItemFactory synchronizedItemFactory, ISynchronizedItemMatcher synchronizedItemMatcher) :
            this(settingsRow, synchronizedItemFactory, synchronizedItemMatcher,
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
            ISynchronizedItem leftItem, ISynchronizedItem rightItem)
        {
            (_settingsRow, _synchronizedItemFactory, _synchronizedItemMatcher) = (settingsRow, synchronizedItemFactory, synchronizedItemMatcher);
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
        /// Директории загружены.
        /// </summary>
        public bool IsLoaded { get; private set; } = false;

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
            await Task.WhenAll(LeftDirectory.Load(), RightDirectory.Load());

            LoadChildItems();
            IsLoaded = true;
            DirectoriesIsLoadedEvent?.Invoke(this);
        }

        /// <summary>
        /// Пометка о том, что требуется загрузка для пары синхронизируемых директорий.
        /// </summary>
        public void LoadRequired()
        {
            IsLoaded = false;
        }

        /// <summary>
        /// Загрузка дочерних записей.
        /// </summary>
        public void LoadChildItems()
        {
            if (ChildItems.Count > 0)
                ChildItems.Clear();

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

                RefreshLeftItemStatusesFromChilds();
                RefreshRightItemStatusesFromChilds();
            }
            else
                _synchronizedItemMatcher.UpdateStatusesAndCommands(LeftItem, RightItem);
        }

        public void IsDeleted()
        {
            DeletedEvent?.Invoke();
        }

        /// <summary>
        /// Обновление статуса левого элемента на основе дочерних.
        /// </summary>
        private void RefreshLeftItemStatusesFromChilds()
        {
            if (ChildItems.Count > 0)
            {
                var notEquallyChilds = ChildItems.Where(r => r.LeftItem.Status.StatusEnum != ItemStatusEnum.Equally).ToArray();

                if (notEquallyChilds.Length == 0)
                {
                    // Если все дочерние строки имеют статус Equally, то и данная строка должна иметь такой сатус, и команд никаких быть при этом не должно.
                    LeftItem.UpdateStatus(ItemStatusEnum.Equally);
                    LeftItem.SyncCommand.SetCommandAction(null);
                }
                else if (notEquallyChilds.Any(r => r.LeftItem.Status.StatusEnum == ItemStatusEnum.Unknown))
                    ItemStatusUnknown(LeftItem);
                else
                {
                    var leftStatuses = notEquallyChilds.Select(r => r.LeftItem.Status.StatusEnum).Distinct().ToArray();

                    if (leftStatuses.Length == 1)
                        SetItemStatusAndCommands(LeftItem, leftStatuses.First(), notEquallyChilds.Select(r => r.LeftItem.SyncCommand.CommandAction));
                    else
                        ItemStatusUnknown(LeftItem);
                }
            }
        }

        /// <summary>
        /// Обновление статуса правого элемента на основе дочерних.
        /// </summary>
        private void RefreshRightItemStatusesFromChilds()
        {
            if (ChildItems.Count > 0)
            {
                var notEquallyChilds = ChildItems.Where(r => r.RightItem.Status.StatusEnum != ItemStatusEnum.Equally).ToArray();

                if (notEquallyChilds.Length == 0)
                {
                    // Если все дочерние строки имеют статус Equally, то и данная строка должна иметь такой сатус, и команд никаких быть при этом не должно.
                    RightItem.UpdateStatus(ItemStatusEnum.Equally);
                    RightItem.SyncCommand.SetCommandAction(null);
                }
                else if (notEquallyChilds.Any(r => r.RightItem.Status.StatusEnum == ItemStatusEnum.Unknown))
                    ItemStatusUnknown(RightItem);
                else
                {
                    var rightStatuses = notEquallyChilds.Select(r => r.RightItem.Status.StatusEnum).Distinct().ToArray();

                    if (rightStatuses.Length == 1)
                        SetItemStatusAndCommands(RightItem, rightStatuses.First(), notEquallyChilds.Select(r => r.RightItem.SyncCommand.CommandAction));
                    else
                        ItemStatusUnknown(RightItem);
                }
            }
        }

        private void ItemStatusUnknown(ISynchronizedItem item)
        {
            item.UpdateStatus(ItemStatusEnum.Unknown);
            item.SyncCommand.SetCommandAction(null);
        }

        private void AddChildItem(ISynchronizedItems child)
        {
            ChildItems.Add(child);
            child.DeleteEvent += DeleteChild;
            child.LeftItem.StatusChangedEvent += RefreshLeftItemStatusesFromChilds;
            child.RightItem.StatusChangedEvent += RefreshRightItemStatusesFromChilds;
        }

        private void DeleteChild(ISynchronizedItems deletingItems)
        {
            DeleteChildWithoutUpdateParent(deletingItems);
            RefreshLeftItemStatusesFromChilds();
            RefreshRightItemStatusesFromChilds();
        }

        private void DeleteChildWithoutUpdateParent(ISynchronizedItems child)
        {
            ChildItems.Remove(child);

            // Раз строка удаляется, больше за ней следить не надо.
            child.DeleteEvent -= DeleteChild;
            child.LeftItem.StatusChangedEvent -= RefreshLeftItemStatusesFromChilds;
            child.RightItem.StatusChangedEvent -= RefreshRightItemStatusesFromChilds;

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
                        childItem = CreateISynchronizedItems(CreateSynchronizedItem(leftItem.FullPath, leftItem is IDirectory, leftItem),
                            CreateSynchronizedItem(rightItem.FullPath, rightItem is IDirectory, rightItem));
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
        /// Создание строки синхронизируемых элементов, в которой не хватает элемента слева.
        /// </summary>
        /// <param name="rightItem">Отслеживаемый правый элемент.</param>
        /// <param name="leftItemDirectory">Путь к директории слева. Нужен, чтобы задать команду удаления.</param>
        /// <returns>Строка синхронизируемых элементов.</returns>
        private ISynchronizedItems LeftMissing(IItem rightItem, string leftItemDirectory)
        {
            var rightSynchronizedItem = CreateSynchronizedItem(rightItem.FullPath, rightItem is IDirectory, rightItem);
            var leftSynchronizedItem = CreateSynchronizedItem(Path.Combine(leftItemDirectory, rightItem.Name), 
                rightSynchronizedItem.IsDirectory, null);

            return CreateISynchronizedItems(leftSynchronizedItem, rightSynchronizedItem);
        }

        /// <summary>
        /// Создание строки синхронизируемых элементов, в которой не хватает элемента справа.
        /// </summary>
        /// <param name="leftItem">Отслеживаемый левый элемент.</param>
        /// <param name="rightItemDirectory">Путь к директории справа. Нужен, чтобы задать команду удаления.</param>
        /// <returns>Строка синхронизируемых элементов.</returns>
        private ISynchronizedItems RightMissing(IItem leftItem, string rightItemDirectory)
        {
            var leftSynchronizedItem = CreateSynchronizedItem(leftItem.FullPath, leftItem is IDirectory, leftItem);
            var rightSynchronizedItem = CreateSynchronizedItem(Path.Combine(rightItemDirectory, leftSynchronizedItem.Name), 
                leftSynchronizedItem.IsDirectory, null);

            return CreateISynchronizedItems(leftSynchronizedItem, rightSynchronizedItem);
        }

        private ISynchronizedItem CreateSynchronizedItem(string itemPath, bool isDirectory, IItem item)
        {
            return isDirectory ? _synchronizedItemFactory.CreateSynchronizedDirectory(itemPath, item as IDirectory)
                : _synchronizedItemFactory.CreateSynchronizedFile(itemPath, item);
        }

        private ISynchronizedItems CreateISynchronizedItems(ISynchronizedItem leftSynchronizedItem, ISynchronizedItem rightSynchronizedItem)
        {
            return new SynchronizedItems(_settingsRow, _synchronizedItemFactory, _synchronizedItemMatcher,
                leftSynchronizedItem, rightSynchronizedItem);
        }

        /// <summary>
        /// Задание статуса и комманд синхронизации для синхронизируемого элемента, исходя из дочерних неидентичных строк.
        /// </summary>
        /// <param name="synchronizedItem">Синхронизируемый элемент, для которого задаётся статус и команды.</param>
        /// <param name="status">Задаваемй статус.</param>
        /// <param name="actionCommands">Команды синхронизации.</param>
        private void SetItemStatusAndCommands(ISynchronizedItem synchronizedItem, ItemStatusEnum status, IEnumerable<Func<Task>> actionCommands)
        {
            synchronizedItem.UpdateStatus(status, _statusCommentsFromChildren.ContainsKey(status) ?
                            _statusCommentsFromChildren[status] : null);

            // Если нет, команды, но должна быть, исходя из дочерних элементов,
            // то можно команду представить как последовательное выпонения команд дочерних элементов. 
            if (status != ItemStatusEnum.Equally)
                synchronizedItem.SyncCommand.SetCommandAction(async () =>
                {
                    foreach (var actionCommand in actionCommands)
                        await actionCommand.Invoke();
                });
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
                await updatedItem.Item.Load();

                // Была выполнена синхронизация, и нам не известно, обновлялись, удалялись или добавлялись дочерние элементы,
                // поэтому удаляем все дочерние элементы и заново создаём.
                foreach (var child in ChildItems.ToArray())
                    DeleteChildWithoutUpdateParent(child);

                LoadChildItems();
                DirectoriesIsLoadedEvent?.Invoke(this);
            }
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