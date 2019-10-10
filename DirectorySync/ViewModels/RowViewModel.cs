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
            RightItem.StartedSyncEvent += StartedSync;
            RightItem.FinishedSyncEvent += FinishedSync;
            RightItem.ItemIsDeletedEvent += Delete;
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
        /// Видимость кнопки команды.
        /// </summary>
        public bool CommandButtonIsVisible => LeftItem.AcceptCommand != null;

        /// <summary>
        /// Видимость заставки процесса.
        /// </summary>
        public bool ProcessIconIsVisible { get; private set; }

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
                var updated = false;
                var statusNames = typeof(ItemStatusEnum).GetEnumNames().Where(n => n != ItemStatusEnum.Unknown.ToString()).ToArray();
                for(byte statusIndex = 0; statusIndex < statusNames.Length && !updated; statusIndex++)
                    if (ChildRows.All(r => r.LeftItem.Status.StatusEnum.ToString() == statusNames[statusIndex]))
                    {
                        LeftItem.UpdateStatus((ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), statusNames[statusIndex]));
                        updated = true;
                    }
                if (!updated)
                    LeftItem.UpdateStatus(ItemStatusEnum.Unknown);

                updated = false;
                for (byte statusIndex = 0; statusIndex < statusNames.Length && !updated; statusIndex++)
                    if (ChildRows.All(r => r.RightItem.Status.StatusEnum.ToString() == statusNames[statusIndex]))
                    {
                        RightItem.UpdateStatus((ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), statusNames[statusIndex]));
                        updated = true;
                    }
                if (!updated)
                    RightItem.UpdateStatus(ItemStatusEnum.Unknown);
            }
        }

        /// <summary>
        /// Реакция на событие загрузки одной из отслеживаемых директорий.
        /// </summary>
        /// <param name="directory">Загруженная директория.</param>
        private void LoadedDirectory(IDirectory directory)
        {
            if (RowViewModelIsLoadedEvent != null && LeftItem.Directory.IsLoaded && RightItem.Directory.IsLoaded)
                RowViewModelIsLoadedEvent.Invoke(this);
        }

        private void StartedSync()
        {
            LeftItem.AcceptCommand = null;
            RightItem.AcceptCommand = null;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CommandButtonIsVisible)));
            ProcessIconIsVisible = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProcessIconIsVisible)));
        }

        private void FinishedSync()
        {
            ItIsOk(this);
            ProcessIconIsVisible = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProcessIconIsVisible)));
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
    }
}