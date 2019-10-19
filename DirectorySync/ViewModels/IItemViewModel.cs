using DirectorySync.Models;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
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
        /// Полный руть к отслеживаемому элементу, который представляет данная модель.
        /// </summary>
        string FullPath { get; }

        /// <summary>
        /// True - элемент является директорией.
        /// </summary>
        bool IsDirectory { get; }

        /// <summary>
        /// Отображаемый моделью элемент синхронизации.
        /// </summary>
        IItem Item { get; }

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
        /// Действия команды синхронизации.
        /// </summary>
        Func<Task> CommandAction { get; }

        /// <summary>
        /// Была изменена команда принятия элемента.
        /// </summary>
        event Action AcceptCommandChangedEvent;

        /// <summary>
        /// Событие запуска синхронизации.
        /// </summary>
        event Action StartedSyncEvent;

        /// <summary>
        /// Событие завершения синхронизации. Передаётся модель представления принятого элемента.
        /// </summary>
        event Action<IItemViewModel> FinishedSyncEvent;

        /// <summary>
        /// Событие, сообщающее о завершении копирования. Передаёт копируемый элемент и элемент, в который осуществлялось копирование.
        /// </summary>
        event Action<IItemViewModel, IItemViewModel> CopiedFromToEvent;

        /// <summary>
        /// Событие возникновения ошибки в процессе синхронизации.
        /// </summary>
        event Action<string> SyncErrorEvent;

        /// <summary>
        /// Обновление статуса.
        /// </summary>
        /// <param name="statusEnum">Новое значение статуса.</param>
        /// <param name="comment">Пояснение к статусу.</param>
        void UpdateStatus(ItemStatusEnum statusEnum, string comment = null);

        /// <summary>
        /// Задание метода, который будет выполняться как команда синхронизации.
        /// </summary>
        /// <param name="action">Метод для синхронизации.</param>
        void SetActionCommand(Func<Task> action);
    }
}