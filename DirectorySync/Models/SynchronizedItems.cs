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

        //private object _isLoadedLocker = new object();

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="settingsRow">Строка настроек, соответствующая синхронизируемым элементам.</param>
        /// <param name="synchronizedItemFactory">Фабрика создания синхронизируемых элементов.</param>
        /// <param name="synchronizedItemMatcher">Объект, выполняющий сравнение синхронизируемых элементов между собой.</param>
        public SynchronizedItems(ISettingsRow settingsRow, ISynchronizedItemFactory synchronizedItemFactory, ISynchronizedItemMatcher synchronizedItemMatcher)
        {
            (_settingsRow, _synchronizedItemFactory, _synchronizedItemMatcher) = (settingsRow, synchronizedItemFactory, synchronizedItemMatcher);

            LeftItem = synchronizedItemFactory.CreateSynchronizedDirectory(settingsRow.LeftDirectory.DirectoryPath, 
                synchronizedItemFactory.CreateDirectory(settingsRow.LeftDirectory.DirectoryPath, settingsRow.ExcludedExtensions));
            RightItem = synchronizedItemFactory.CreateSynchronizedDirectory(settingsRow.RightDirectory.DirectoryPath, 
                synchronizedItemFactory.CreateDirectory(settingsRow.RightDirectory.DirectoryPath, settingsRow.ExcludedExtensions));
        }

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
            ISynchronizedItem leftItem, ISynchronizedItem rightItem, ISynchronizedItems parentDirectories) : this(settingsRow, synchronizedItemFactory, synchronizedItemMatcher)
        {
            (LeftItem, RightItem, ParentDirectories) = (leftItem, rightItem, parentDirectories);
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
        /// Пара родительских синхронизируемых директорий.
        /// </summary>
        public ISynchronizedItems ParentDirectories { get; }

        /// <summary>
        /// Событие, возникающее при полной загрузке обоих директорий. Передаётся текущая модель.
        /// </summary>
        public event Action<ISynchronizedItems> DirectoriesIsLoadedEvent;

        /// <summary>
        /// Загрузка директорий.
        /// </summary>
        public async Task Load()
        {
            //LeftDirectory.LoadedDirectoryEvent += DirectoryIsLoaded;
            //RightDirectory.LoadedDirectoryEvent += DirectoryIsLoaded;

            await LeftDirectory.Load();
            await RightDirectory.Load();

            //DirectoryIsLoaded();
            RefreshChildItems();
            RefreshParentStatuses(this);
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

        //private void DirectoryIsLoaded()
        //{
        //    //directory.LoadedDirectoryEvent -= DirectoryIsLoaded;

        //    // Это на случай, когда одновремено загружаются обе директории, чтобы два раза не выполнять методы Refresh.
        //    var ok = false;
        //    lock (_isLoadedLocker)
        //    {
        //        ok = LeftDirectory.IsLoaded && RightDirectory.IsLoaded && !IsLoaded;
        //        IsLoaded = true;
        //    }

        //    if (ok)
        //    {
        //        RefreshChildDirectories();
        //        RefreshParentStatuses(this);
        //        DirectoriesIsLoadedEvent?.Invoke(this);
        //    }
        //}

        /// <summary>
        /// Обновление дочерних записей.
        /// </summary>
        public void RefreshChildItems()
        {
            if (ChildItems.Count > 0)
                ChildItems.Clear();

            if (LeftDirectory != null && RightDirectory != null)
            {
                foreach (var directories in CreateChildItems(
                    LeftDirectory.Items.Where(i => i is IDirectory).ToArray(), RightDirectory.Items.Where(i => i is IDirectory).ToArray()))
                {
                    ChildItems.Add(directories);
                    directories.RefreshChildItems();
                }

                ChildItems.AddRange(CreateChildItems(
                    LeftDirectory.Items.Where(i => !(i is IDirectory)).ToArray(), RightDirectory.Items.Where(i => !(i is IDirectory)).ToArray()));

                RefreshStatusesFromChilds();
            }
            else
                _synchronizedItemMatcher.UpdateStatusesAndCommands(LeftItem, RightItem);
        }

        /// <summary>
        /// Обновление статусов на основе дочерних записей.
        /// </summary>
        public void RefreshStatusesFromChilds()
        {
            if (ChildItems.Count > 0)
            {
                // Достаточно проверять статус с одной стороны, так как если с одной стороны Equally, то и с другой стороны обязательно Equally.
                var notEquallyChilds = ChildItems.Where(r => r.LeftItem.Status.StatusEnum != ItemStatusEnum.Equally).ToArray();

                if (notEquallyChilds.Length == 0)
                {
                    // Если все дочерние строки имеют статус Equally, то и данная строка должна иметь такой сатус, и команд никаких быть при этом не должно.
                    LeftItem.UpdateStatus(ItemStatusEnum.Equally);
                    LeftItem.SyncCommand.SetCommandAction(null);
                    RightItem.UpdateStatus(ItemStatusEnum.Equally);
                    RightItem.SyncCommand.SetCommandAction(null);
                }
                else if (notEquallyChilds.Any(r => r.LeftItem.Status.StatusEnum == ItemStatusEnum.Unknown))
                {
                    // Если хоть одна дочерняя строка имеет статус Unknown, то и данная строка должна иметь такой сатус, и команд никаких быть при этом не должно.
                    LeftItem.UpdateStatus(ItemStatusEnum.Unknown);
                    LeftItem.SyncCommand.SetCommandAction(null);
                    RightItem.UpdateStatus(ItemStatusEnum.Unknown);
                    RightItem.SyncCommand.SetCommandAction(null);
                }
                else
                {
                    var leftStatuses = notEquallyChilds.Select(r => r.LeftItem.Status.StatusEnum).Distinct().ToArray();

                    // Если с одной стороны все элементы имеют один статус, то и с другой тоже.
                    if (leftStatuses.Length == 1)
                    {
                        SetItemStatusAndCommands(LeftItem, leftStatuses.First(), notEquallyChilds.Select(r => r.LeftItem.SyncCommand.CommandAction));
                        SetItemStatusAndCommands(RightItem, notEquallyChilds.First().RightItem.Status.StatusEnum, notEquallyChilds.Select(r => r.RightItem.SyncCommand.CommandAction));
                    }
                    else
                    {
                        LeftItem.UpdateStatus(ItemStatusEnum.Unknown);
                        RightItem.UpdateStatus(ItemStatusEnum.Unknown);
                    }
                }
            }
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

                _synchronizedItemMatcher.UpdateStatusesAndCommands(childItem.LeftItem, childItem.RightItem);
                result.Add(childItem);
            }

            // Если с правой стороны элементов оказалось больше.
            for (; rightItemIndex < rightItems.Length; rightItemIndex++)
            {
                var childItem = LeftMissing(rightItems[rightItemIndex], LeftDirectory.FullPath);
                _synchronizedItemMatcher.UpdateStatusesAndCommands(childItem.LeftItem, childItem.RightItem);
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
                leftSynchronizedItem, rightSynchronizedItem, this);
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

        /// <summary>
        /// Обновление статусов всех родительских строк.
        /// </summary>
        /// <param name="synchronizedItems">Строка, у родителя которой будет обновлён статус.</param>
        private void RefreshParentStatuses(ISynchronizedItems synchronizedItems)
        {
            if (synchronizedItems.ParentDirectories != null)
            {
                synchronizedItems.ParentDirectories.RefreshStatusesFromChilds();
                RefreshParentStatuses(synchronizedItems.ParentDirectories);
            }
        }
    }
}