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
        ICommand AcceptCommand { get; set; }

        /// <summary>
        /// Событие запуска синхронизации.
        /// </summary>
        event Action StartedSyncEvent;

        /// <summary>
        /// Событие завершения синхронизации.
        /// </summary>
        event Action FinishedSyncEvent;

        /// <summary>
        /// Обновление статуса.
        /// </summary>
        /// <param name="statusEnum">Новое значение статуса.</param>
        void UpdateStatus(ItemStatusEnum statusEnum);
    }
}