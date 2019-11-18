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
        /// Событие возникновения ошибки в процессе синхронизации.
        /// </summary>
        event Action<string> SyncErrorEvent;

        /// <summary>
        /// Отобразить, что идёт обновление строки.
        /// </summary>
        void ShowInProcess();

        /// <summary>
        /// Увдеомление о завершении загрузки строки. 
        /// </summary>
        void LoadFinished();
    }
}