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
        /// Дочерние строки.
        /// </summary>
        ObservableCollection<IRowViewModel> ChildRows { get; }

        /// <summary>
        /// Видимость кнопки команды.
        /// </summary>
        bool CommandButtonIsVisible { get; }

        /// <summary>
        /// Видимость заставки процесса.
        /// </summary>
        bool ProcessIconIsVisible { get; }

        /// <summary>
        /// Событие, возникающее при полной загрузке входящих в строку элементов.
        /// </summary>
        event Action<IRowViewModel> RowViewModelIsLoadedEvent;

        /// <summary>
        /// Событие возникает, когда строка должна быть удалена. Указывается какая строка удаляется и из какой строки она удаляется.
        /// </summary>
        event Action<IRowViewModel, IRowViewModel> DeleteRowViewModelEvent;

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