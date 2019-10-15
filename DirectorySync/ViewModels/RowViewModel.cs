using DirectorySync.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace DirectorySync.ViewModels
{
    /// <summary>
    /// Строка представления отслеживаемых элементов.
    /// </summary>
    public class RowViewModel : IRowViewModel
    {
        private bool _isExpanded;
        private bool _isSelected;
        private bool _processIconIsVisible = true;

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
            if (LeftItem.Directory != null)
                LeftItem.Directory.LoadedDirectoryEvent += LoadedDirectory;
            if (RightItem.Directory != null)
                RightItem.Directory.LoadedDirectoryEvent += LoadedDirectory;

            LeftItem.StartedSyncEvent += StartedSync;
            LeftItem.FinishedSyncEvent += FinishedSync;
            LeftItem.ItemIsDeletedEvent += Delete;
            LeftItem.CopiedFromToEvent += CopiedFromTo;
            LeftItem.AcceptCommandChangedEvent += AcceptCommandChanged;
            RightItem.StartedSyncEvent += StartedSync;
            RightItem.FinishedSyncEvent += FinishedSync;
            RightItem.ItemIsDeletedEvent += Delete;
            RightItem.CopiedFromToEvent += CopiedFromTo;
            RightItem.AcceptCommandChangedEvent += AcceptCommandChanged;
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
        public bool CommandButtonIsVisible => LeftItem.AcceptCommand != null;

        /// <summary>
        /// Видимость заставки процесса.
        /// </summary>
        public bool ProcessIconIsVisible
        {
            get { return _processIconIsVisible; }
            private set
            {
                if(_processIconIsVisible!=value)
                {
                    _processIconIsVisible = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProcessIconIsVisible)));
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
        /// Событие возникает, когда строка должна быть удалена. Указывается какая строка удаляется и из какой строки она удаляется.
        /// </summary>
        public event Action<IRowViewModel, IRowViewModel> DeleteRowViewModelEvent;

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
                        LeftItem.UpdateStatus(leftStatuses.First());

                        // Если нет, команды, но должна быть, исходя из дочерних элементов,
                        // то можно команду представить как последовательное выпонения команд дочерних элементов. 
                        if (LeftItem.Status.StatusEnum != ItemStatusEnum.Equally && LeftItem.AcceptCommand == null)
                            LeftItem.SetActionCommand(() =>
                            {
                                foreach (var actionCommand in notEquallyChilds.Select(r => r.LeftItem.CommandAction))
                                    actionCommand.Invoke();
                            });

                        RightItem.UpdateStatus(notEquallyChilds.First().RightItem.Status.StatusEnum);

                        // Если нет, команды, но должна быть, исходя из дочерних элементов,
                        // то можно команду представить как последовательное выпонения команд дочерних элементов. 
                        if (RightItem.Status.StatusEnum != ItemStatusEnum.Equally && RightItem.AcceptCommand == null)
                            RightItem.SetActionCommand(() =>
                            {
                                foreach (var actionCommand in notEquallyChilds.Select(r => r.RightItem.CommandAction))
                                    actionCommand.Invoke();
                            });
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
        /// Реакция на событие загрузки одной из отслеживаемых директорий.
        /// </summary>
        /// <param name="directory">Загруженная директория.</param>
        private void LoadedDirectory(IDirectory directory)
        {
            if (RowViewModelIsLoadedEvent != null && LeftItem.Directory.IsLoaded && RightItem.Directory.IsLoaded)
            {
                RowViewModelIsLoadedEvent.Invoke(this);
                ProcessIconIsVisible = false;
            }
        }

        private void StartedSync()
        {
            LeftItem.SetActionCommand(null);
            RightItem.SetActionCommand(null);
            ProcessIconIsVisible = true;
        }

        private void FinishedSync()
        {
            //ItIsOk(this);
            ProcessIconIsVisible = false;
        }

        private void ItIsOk(IRowViewModel rowViewModel)
        {
            rowViewModel.LeftItem.UpdateStatus(ItemStatusEnum.Equally);
            rowViewModel.RightItem.UpdateStatus(ItemStatusEnum.Equally);
            foreach (var childRow in rowViewModel.ChildRows)
                ItIsOk(childRow);
        }

        private void Delete()
        {
            DeleteRowViewModelEvent?.Invoke(this, Parent);
        }

        private void CopiedFromTo(IItemViewModel fromItem, IItemViewModel toItem)
        {
            if (LeftItem == fromItem)
                RightItem = toItem;
            else
                LeftItem = toItem;

            if (toItem.IsDirectory)
                toItem.Directory.Load().Wait();

            RowViewModelIsLoadedEvent?.Invoke(this);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LeftItem)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RightItem)));
        }

        private void AcceptCommandChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CommandButtonIsVisible)));
        }
    }
}