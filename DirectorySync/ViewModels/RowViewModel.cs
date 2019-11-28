using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DirectorySync.ViewModels
{
    /// <summary>
    /// Строка представления отслеживаемых элементов.
    /// </summary>
    public class RowViewModel : IRowViewModel
    {
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
            private set
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
        /// Событие изменения одного из свойств модели.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Событие возникновения ошибки в процессе синхронизации.
        /// </summary>
        public event Action<string> SyncErrorEvent;

        /// <summary>
        /// Отобразить, что строка в процессе обновления.
        /// </summary>
        public void ShowInProcess()
        {
            InProcess = true;
            foreach (var child in ChildRows)
                child.ShowInProcess();
        }

        /// <summary>
        /// Убрать отображение процесса обновления. 
        /// </summary>
        public void HideInProcess()
        {
            InProcess = false;
            foreach (var child in ChildRows)
                child.HideInProcess();
        }

        private void AcceptCommandChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CommandButtonIsVisible)));
        }

        private void SetItemViewModelEvents(IItemViewModel itemViewModel)
        {
            itemViewModel.StartedSyncEvent += ShowInProcess;
            itemViewModel.AcceptCommandChangedEvent += AcceptCommandChanged;
            itemViewModel.SyncErrorEvent += (string error) => { SyncErrorEvent?.Invoke(error); };
        }
    }
}