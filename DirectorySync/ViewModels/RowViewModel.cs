﻿using DirectorySync.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace DirectorySync.ViewModels
{
    /// <summary>
    /// Строка представления отслеживаемых элементов.
    /// </summary>
    public class RowViewModel : IRowViewModel
    {
        private static readonly Dictionary<ItemStatusEnum, string> _statusCommentsFromChildren = new Dictionary<ItemStatusEnum, string>
            {
                {ItemStatusEnum.Missing, "Не хватает тех элементов, что есть с другой стороны"},
                {ItemStatusEnum.ThereIs, "Содержит отсутствующие с другой стороны элементы"},
                {ItemStatusEnum.Older, "Содержит более старые"},
                {ItemStatusEnum.Newer, "Содержит более новые"}
            };

        private bool _isExpanded;
        private bool _isSelected;
        private bool _inProcess = true;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="leftItem">Элемент слева.</param>
        /// <param name="rightItem">Элемент справа.</param>
        public RowViewModel(IItemViewModel leftItem, IItemViewModel rightItem, IRowViewModel parent)
        {
            LeftItem = leftItem;
            RightItem = rightItem;
            Parent = parent;
            ChildRows = new ObservableCollection<IRowViewModel>();

            SetItemViewModelEvents(LeftItem);
            SetItemViewModelEvents(RightItem);
        }

        /// <summary>
        /// Элемент слева.
        /// </summary>
        public IItemViewModel LeftItem { get; private set; }

        /// <summary>
        /// Элемент справа.
        /// </summary>
        public IItemViewModel RightItem { get; private set; }

        /// <summary>
        /// True - дочерние элементы строки показаны.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
            }
        }

        /// <summary>
        /// True - строка выбрана в данный момент.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        /// <summary>
        /// True - строка содержит элементы, описывающие директории.
        /// </summary>
        public bool IsDirectory => LeftItem != null && LeftItem.IsDirectory || RightItem != null && RightItem.IsDirectory;

        /// <summary>
        /// Видимость кнопки команды.
        /// </summary>
        public bool CommandButtonIsVisible =>
            LeftItem.AcceptCommand != null && RightItem.AcceptCommand != null && !InProcess;

        /// <summary>
        /// True - строка находится в процессе обновления.
        /// </summary>
        public bool InProcess
        {
            get { return _inProcess; }
            set
            {
                if(_inProcess!=value)
                {
                    _inProcess = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InProcess)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CommandButtonIsVisible)));
                }
            }
        }

        /// <summary>
        /// Дочерние строки.
        /// </summary>
        public ObservableCollection<IRowViewModel> ChildRows { get; private set; }

        /// <summary>
        /// Строка, куда входит данная строка.
        /// </summary>
        public IRowViewModel Parent { get; }

        /// <summary>
        /// Событие изменения одного из свойств модели.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Событие, возникающее при полной загрузке входящих в строку элементов.
        /// </summary>
        public event Action<IRowViewModel> RowViewModelIsLoadedEvent;

        /// <summary>
        /// Событие возникновения ошибки в процессе синхронизации.
        /// </summary>
        public event Action<string> SyncErrorEvent;

        /// <summary>
        /// Обновление дочерних строк.
        /// </summary>
        /// <param name="rows">Новые дочерние строки.</param>
        public void RefreshChildRows(IRowViewModel[] rows)
        {
            ChildRows = new ObservableCollection<IRowViewModel>(rows);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChildRows)));
        }

        /// <summary>
        /// Проставить статусы отслеживаемых элементов на основании статусов дочерних элементов.
        /// </summary>
        public void RefreshStatusesFromChilds()
        {
            if (ChildRows.Count > 0)
            {
                var notEquallyChilds = ChildRows.Where(r => r.LeftItem.Status.StatusEnum != ItemStatusEnum.Equally).ToArray();

                if (notEquallyChilds.Length == 0)
                {
                    LeftItem.UpdateStatus(ItemStatusEnum.Equally);
                    LeftItem.SetActionCommand(null);
                    RightItem.UpdateStatus(ItemStatusEnum.Equally);
                    RightItem.SetActionCommand(null);
                }
                else
                {
                    var leftStatuses = notEquallyChilds.Select(r => r.LeftItem.Status.StatusEnum).Distinct().ToArray();

                    // Если с одной стороны все элементы имеют один статус, то и с другой тоже.
                    if (leftStatuses.Length == 1)
                    {
                        SetItemStatusAndCommands(LeftItem, leftStatuses.First(), notEquallyChilds.Select(r => r.LeftItem.CommandAction));
                        SetItemStatusAndCommands(RightItem, notEquallyChilds.First().RightItem.Status.StatusEnum, notEquallyChilds.Select(r => r.RightItem.CommandAction));
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
        /// Отобразить, что идёт обновление строки.
        /// </summary>
        public void ShowInProcess()
        {
            InProcess = true;
            SetInProcessForChildren(this, true);
        }

        /// <summary>
        /// Задание статуса и комманд синхронизации для модели представления элемента, исходя из дочерних неидентичных строк.
        /// </summary>
        /// <param name="itemViewModel">Модель представления элемента, для которой задаётся статус и команды.</param>
        /// <param name="status">Задаваемй статус.</param>
        /// <param name="actionCommands">Команды синхронизации.</param>
        private void SetItemStatusAndCommands(IItemViewModel itemViewModel, ItemStatusEnum status, IEnumerable<Func<Task>> actionCommands)
        {
            itemViewModel.UpdateStatus(status, _statusCommentsFromChildren.ContainsKey(status) ?
                            _statusCommentsFromChildren[status] : null);

            // Если нет, команды, но должна быть, исходя из дочерних элементов,
            // то можно команду представить как последовательное выпонения команд дочерних элементов. 
            if (status != ItemStatusEnum.Equally)
                itemViewModel.SetActionCommand(async () =>
                {
                    foreach (var actionCommand in actionCommands)
                        await actionCommand.Invoke();
                });
        }

        /// <summary>
        /// Реакция на событие загрузки одной из отслеживаемых директорий.
        /// </summary>
        /// <param name="directory">Загруженная директория.</param>
        private void LoadedDirectory(IDirectory directory)
        {
            if((LeftItem.Directory == null || LeftItem.Directory.IsLoaded) && (RightItem.Directory == null || RightItem.Directory.IsLoaded))
            {
                RowViewModelIsLoadedEvent?.Invoke(this);
                InProcess = false;
                SetInProcessForChildren(this, false);
            }
        }

        private void SetInProcessForChildren(IRowViewModel row, bool inProcessValue)
        {
            foreach(var child in row.ChildRows)
            {
                child.InProcess = inProcessValue;
                SetInProcessForChildren(child, inProcessValue);
            }
        }

        private void StartedSync()
        {
            InProcess = true;
        }

        private void FinishedSync(IItemViewModel acceptedItem)
        {
            var refreshItem = LeftItem == acceptedItem ? RightItem : LeftItem;
            if (refreshItem.Directory != null)
                refreshItem.Directory.Load().Wait();
            else if (LeftItem.Item == null && RightItem.Item == null)
            {
                // Если обоих элементов уже нет, пусть обновляется родительский элемент, чтобы убралась эта строка.
                RowViewModelIsLoadedEvent?.Invoke(Parent);
                SetInProcessForChildren(Parent, false);
            }
            else
            {
                (refreshItem == LeftItem ? Parent.LeftItem : Parent.RightItem).Directory?.Load().Wait();
                RowViewModelIsLoadedEvent?.Invoke(this);
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LeftItem)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RightItem)));
            InProcess = false;
        }

        private void CopiedFromTo(IItemViewModel fromItem, IItemViewModel toItem)
        {
            if (LeftItem == fromItem)
            {
                if (RightItem.Directory == null && toItem.Directory != null)
                    // Если модели директории не было, то и на событие загрузки подписи не было. А теперь должна быть.
                    toItem.Directory.LoadedDirectoryEvent += LoadedDirectory;
                RightItem = toItem;
            }
            else
            {
                if (LeftItem.Directory == null && toItem.Directory != null)
                    // Если модели директории не было, то и на событие загрузки подписи не было. А теперь должна быть.
                    LeftItem.Directory.LoadedDirectoryEvent += LoadedDirectory;
                LeftItem = toItem;
            }
        }

        private void AcceptCommandChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CommandButtonIsVisible)));
        }

        private void SetItemViewModelEvents(IItemViewModel itemViewModel)
        {
            if (itemViewModel.Directory != null)
                itemViewModel.Directory.LoadedDirectoryEvent += LoadedDirectory;

            itemViewModel.StartedSyncEvent += StartedSync;
            itemViewModel.FinishedSyncEvent += FinishedSync;
            itemViewModel.CopiedFromToEvent += CopiedFromTo;
            itemViewModel.AcceptCommandChangedEvent += AcceptCommandChanged;
            itemViewModel.SyncErrorEvent += (string error) => { SyncErrorEvent?.Invoke(error); };
        }
    }
}