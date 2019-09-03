using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DirectorySync.ViewModels
{
    /// <summary>
    /// Строка представления отслеживаемых элементов.
    /// </summary>
    public interface IRowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Элемент слева.
        /// </summary>
        IItemViewModel LeftItem { get; }

        /// <summary>
        /// Элемент справа.
        /// </summary>
        IItemViewModel RightItem { get; }

        /// <summary>
        /// True - дочерние строки скрыты.
        /// </summary>
        bool Collapsed { get; set; }

        /// <summary>
        /// Дочерние строки.
        /// </summary>
        ObservableCollection<IRowViewModel> ChildRows { get; }

        /// <summary>
        /// Событие, возникающее при полной загрузке входящих в строку элементов.
        /// </summary>
        event SynchronizedItemsViewModelIsLoaded SynchronizedItemsViewModelIsLoadedEvent;

        /// <summary>
        /// Обновление дочерних строк.
        /// </summary>
        /// <param name="rows">Новые дочерние строки.</param>
        void RefreshChildRows(IRowViewModel[] rows);
    }
}