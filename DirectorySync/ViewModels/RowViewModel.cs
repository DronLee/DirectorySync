using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DirectorySync.ViewModels
{
    /// <summary>
    /// Строка представления отслеживаемых элементов.
    /// </summary>
    public class RowViewModel : IRowViewModel
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="leftItem">Элемент слева.</param>
        /// <param name="rightItem">Элемент справа.</param>
        public RowViewModel(IItemViewModel leftItem, IItemViewModel rightItem)
        {
            LeftItem = leftItem;
            RightItem = rightItem;
            ChildRows = new ObservableCollection<IRowViewModel>();
            if (LeftItem.Directory != null)
                LeftItem.Directory.LoadedDirectoryEvent += LoadedDirectory;
            if (RightItem.Directory != null)
                RightItem.Directory.LoadedDirectoryEvent += LoadedDirectory;
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
        /// True - дочерние строки скрыты.
        /// </summary>
        public bool Collapsed { get; set; }

        /// <summary>
        /// Дочерние строки.
        /// </summary>
        public ObservableCollection<IRowViewModel> ChildRows { get; private set; }

        /// <summary>
        /// Событие изменения одного из свойств модели.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Событие, возникающее при полной загрузке входящих в строку элементов.
        /// </summary>
        public event RowViewModelIsLoaded RowViewModelIsLoadedEvent;

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
        /// Реакция на событие загрузки одной из отслеживаемых директорий.
        /// </summary>
        /// <param name="directory">Загруженная директория.</param>
        private void LoadedDirectory(Models.IDirectory directory)
        {
            if (RowViewModelIsLoadedEvent != null && LeftItem.Directory.IsLoaded && RightItem.Directory.IsLoaded)
                RowViewModelIsLoadedEvent.Invoke(this);
        }
    }
}