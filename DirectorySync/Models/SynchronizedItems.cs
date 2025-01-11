using DirectorySync.Models.Settings;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
        private readonly ISynchronizedItemsStatusAndCommandsUpdater _statusAndCommandsUpdater;

        private readonly List<ISynchronizedItems> _childItems = new List<ISynchronizedItems>();
        private readonly ReaderWriterLockSlim _childItemsLock = new ReaderWriterLockSlim();
        
        private readonly ILogger _logger;

        private bool _inProcess = false;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="settingsRow">Строка настроек, соответствующая синхронизируемым элементам.</param>
        /// <param name="synchronizedItemFactory">Фабрика создания синхронизируемых элементов.</param>
        /// <param name="synchronizedItemMatcher">Объект, выполняющий сравнение синхронизируемых элементов между собой.</param>
        public SynchronizedItems(ISettingsRow settingsRow, ISynchronizedItemFactory synchronizedItemFactory,
            ISynchronizedItemsStatusAndCommandsUpdater statusAndCommandsUpdater, ILogger logger) :
            this(settingsRow, synchronizedItemFactory, statusAndCommandsUpdater,
                synchronizedItemFactory.CreateSynchronizedDirectory(settingsRow.LeftDirectory.DirectoryPath,
                    synchronizedItemFactory.CreateDirectory(settingsRow.LeftDirectory.DirectoryPath, settingsRow.ExcludedExtensions)),
                synchronizedItemFactory.CreateSynchronizedDirectory(settingsRow.RightDirectory.DirectoryPath,
                    synchronizedItemFactory.CreateDirectory(settingsRow.RightDirectory.DirectoryPath, settingsRow.ExcludedExtensions)),
                logger)
        {
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
        private SynchronizedItems(ISettingsRow settingsRow, ISynchronizedItemFactory synchronizedItemFactory,
            ISynchronizedItemsStatusAndCommandsUpdater statusAndCommandsUpdater, ISynchronizedItem leftItem, ISynchronizedItem rightItem, ILogger logger)
        {
            (_settingsRow, _synchronizedItemFactory, _statusAndCommandsUpdater, _logger) =
                (settingsRow, synchronizedItemFactory, statusAndCommandsUpdater, logger);
            (LeftItem, RightItem) = (leftItem, rightItem);

            SetEventsSynchronizedItem(LeftItem);
            SetEventsSynchronizedItem(RightItem);
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
        public ISynchronizedItems[] ChildItems
        {
            get
            {
                _childItemsLock.EnterReadLock();
                try
                {
                    return _childItems.ToArray();
                }
                finally
                {
                    _childItemsLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Количество дочерних элементов.
        /// </summary>
        public int ChildItemsCount 
        { 
            get
            {
                _childItemsLock.EnterReadLock();
                try
                {
                    return _childItems.Count;
                }
                finally
                {
                    _childItemsLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// True - элементы синхронизации находятся в процессе загрузки или синхронизации.
        /// </summary>
        public bool InProcess
        {
            get { return _inProcess; }
            set
            {
                if (_inProcess != value)
                {
                    _inProcess = value;
                    InProcessChangedEvent?.Invoke(value);
                }
            }
        }

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
        /// Событие оповещает, что признак InProcess был изменён и передаёт новое значение.
        /// </summary>
        public event Action<bool> InProcessChangedEvent;

        /// <summary>
        /// Загрузка директорий.
        /// </summary>
        public async Task Load()
        {
            InProcessChange(true);

            StartLoadDirectoriesEvent?.Invoke();

            await Task.WhenAll(LeftDirectory.Load(), RightDirectory.Load());

            ClearChildItems();
            LoadChildItems();

            DirectoriesIsLoadedEvent?.Invoke(this);

            InProcessChange(false);

            _logger.Information("Завершена загрузка директорий:\n{0}\n{1}", LeftDirectory.FullPath, RightDirectory.FullPath);
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
                    _statusAndCommandsUpdater.UpdateStatusesAndCommands(file.LeftItem, file.RightItem);
                    AddChildItem(file);
                }

                RefreshLeftItemStatusAndCommands();
                RefreshRightItemStatusAndCommands();
            }
            else
                _statusAndCommandsUpdater.UpdateStatusesAndCommands(LeftItem, RightItem);
        }

        /// <summary>
        /// Оповещение об удалении элемента.
        /// </summary>
        public void IsDeleted()
        {
            DeletedEvent?.Invoke();
        }

        /// <summary>
        /// Поменять значение признака InProcess у текущих элементов и у всех дочерних.
        /// </summary>
        public void InProcessChange(bool inProcess)
        {
            InProcess = inProcess;
            foreach (var child in ChildItems)
            {
                child.InProcessChangedEvent -= ChildInProcessChanged; // Не надо реагировать на событие, если сами и меняем значение.
                child.InProcessChange(inProcess);
                child.InProcessChangedEvent += ChildInProcessChanged;
            }
        }

        private void SetEventsSynchronizedItem(ISynchronizedItem synchronizedItem)
        {
            if (synchronizedItem.Item != null)
                synchronizedItem.Item.DeletedEvent += ItemDeleted;
            synchronizedItem.FinishedSyncEvent += FinishedSync;
            synchronizedItem.CopiedFromToEvent += CopiedFromItem;
            synchronizedItem.StartedSyncEvent += () => InProcessChange(true);
        }

        private void ChildInProcessChanged(bool inProcess)
        {
            if (inProcess)
                InProcess = true;
            else
            {
                bool allChildItemsNotInProcess;
                _childItemsLock.EnterReadLock();
                try
                {
                    allChildItemsNotInProcess = _childItems.All(i => !i.InProcess);
                }
                finally
                {
                    _childItemsLock.ExitReadLock();
                }

                if (allChildItemsNotInProcess)
                    InProcess = false;
            }
        }

        private void AddChildItem(ISynchronizedItems child)
        {
            _childItemsLock.EnterWriteLock();
            try
            {
                _childItems.Add(child);
            }
            finally
            {
                _childItemsLock.ExitWriteLock();
            }

            child.DeleteEvent += DeleteChild;
            child.LeftItem.StatusChangedEvent += RefreshLeftItemStatusAndCommands;
            child.RightItem.StatusChangedEvent += RefreshRightItemStatusAndCommands;
            child.InProcessChangedEvent += ChildInProcessChanged;
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
            _childItemsLock.EnterWriteLock();
            try
            {
                _childItems.Remove(child);
            }
            finally
            {
                _childItemsLock.ExitWriteLock();
            }

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
            return new SynchronizedItems(_settingsRow, _synchronizedItemFactory, _statusAndCommandsUpdater,
                leftSynchronizedItem, rightSynchronizedItem, _logger);
        }

        private void ItemDeleted(IItem item)
        {
            item.DeletedEvent -= ItemDeleted; // Раз удалился, значит больше не удалится.
            DeleteEvent?.Invoke(this);
        }

        private void FinishedSync(ISynchronizedItem synchronizedItem)
        {
            var updatedItem = LeftItem == synchronizedItem ? RightItem : LeftItem;

            // Если Item == null, значит элемент удалили, и тут больше делать ничего не надо.
            if (updatedItem.Item != null)
            {
                StartLoadDirectoriesEvent?.Invoke();
                updatedItem.Item.Load().Wait();

                // Была выполнена синхронизация, и нам не известно, обновлялись, удалялись или добавлялись дочерние элементы,
                // поэтому заново загрузим дочерние элементы удаляем все дочерние элементы и заново загружаем.
                ClearChildItems();
                LoadChildItems();

                DirectoriesIsLoadedEvent?.Invoke(this);
            }

            InProcessChange(false);
        }

        private void ClearChildItems()
        {
            foreach (var child in ChildItems)
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