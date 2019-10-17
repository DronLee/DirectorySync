using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DirectorySync.ViewModels
{
    /// <summary>
    /// Интерфейс строки представления отслеживаемых элементов.
    /// </summary>
    public interface IRowViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Строка, куда входит данная строка.
        /// </summary>
        IRowViewModel Parent { get; }

        /// <summary>
        /// Элемент слева.
        /// </summary>
        IItemViewModel LeftItem { get; }

        /// <summary>
        /// Элемент справа.
        /// </summary>
        IItemViewModel RightItem { get; }

        /// <summary>
        /// True - строка выбрана в данный момент.
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// True - дочерние элементы строки показаны.
        /// </summary>
        bool IsExpanded { get; set; }

        /// <summary>
        /// True - строка содержит элементы, описывающие директории.
        /// </summary>
        bool IsDirectory { get; }

        /// <summary>
        /// Дочерние строки.
        /// </summary>
        ObservableCollection<IRowViewModel> ChildRows { get; }

        /// <summary>
        /// Видимость кнопки команды.
        /// </summary>
        bool CommandButtonIsVisible { get; }

        /// <summary>
        /// True - строка находится в процессе обновления.
        /// </summary>
        bool InProcess { get; set; }

        /// <summary>
        /// Событие, возникающее при полной загрузке входящих в строку элементов.
        /// </summary>
        event Action<IRowViewModel> RowViewModelIsLoadedEvent;

        /// <summary>
        /// Событие возникновения ошибки в процессе синхронизации.
        /// </summary>
        event Action<string> SyncErrorEvent;

        /// <summary>
        /// Обновление дочерних строк.
        /// </summary>
        /// <param name="rows">Новые дочерние строки.</param>
        void RefreshChildRows(IRowViewModel[] rows);

        /// <summary>
        /// Проставить статусы отслеживаемых элементов на основании статусов дочерних элементов.
        /// </summary>
        void RefreshStatusesFromChilds();
    }
}