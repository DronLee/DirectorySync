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
    }
}