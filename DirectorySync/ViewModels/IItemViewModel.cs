using DirectorySync.Models;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace DirectorySync.ViewModels
{
    /// <summary>
    /// Интерфейс модели представления отслеживаемого элемента.
    /// </summary>
    public interface IItemViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Наименование.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// True - элемент является директорией.
        /// </summary>
        bool IsDirectory { get; }

        /// <summary>
        /// Отображаемая моделью директория. Если модель отображает файл, то null.
        /// </summary>
        IDirectory Directory { get; }

        /// <summary>
        /// Статус элемента.
        /// </summary>
        ItemStatus Status { get; }
        
        /// <summary>
        /// Путь к иконке отслеживаемого элемента.
        /// </summary>
        string IconPath { get; }

        /// <summary>
        /// Выполняемая команда синхронизации. 
        /// </summary>
        ICommand AcceptCommand { get; }

        /// <summary>
        /// Событие запуска синхронизации.
        /// </summary>
        event Action StartedSyncEvent;

        /// <summary>
        /// Событие возникновения ошибки в процессе синхронизации.
        /// </summary>
        event Action<string> SyncErrorEvent;

        /// <summary>
        /// Событие изменения команды синхронизации.
        /// </summary>
        event Action AcceptCommandChangedEvent;
    }
}