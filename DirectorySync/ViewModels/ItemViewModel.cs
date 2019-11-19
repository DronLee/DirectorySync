using System;
using System.ComponentModel;
using System.Windows.Input;
using DirectorySync.Models;

namespace DirectorySync.ViewModels
{
    /// <summary>
    /// Модель представления отслеживаемого элемента.
    /// </summary>
    public class ItemViewModel : IItemViewModel
    {
        private const string _fileIconPath = "/DirectorySync;component/Icons/File.png";
        private const string _folderIconPath = "/DirectorySync;component/Icons/Folder.png";

        private readonly ISynchronizedItem _synchronizedItem;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="name">Наименование отображаемого элемента.</param>
        /// <param name="isDirectory">True - присутствующий элемент является директорией.</param>
        /// <param name="item">Отслеживаемый элемент, на основе которого создаётся модель.</param>
        public ItemViewModel(ISynchronizedItem synchronizedItem)
        {
            _synchronizedItem = synchronizedItem;
            _synchronizedItem.StatusChangedEvent += () => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status))); };
            _synchronizedItem.SyncCommand.CommandActionChangedEvent += () =>
            {
                if (_synchronizedItem.SyncCommand.CommandAction == null)
                    AcceptCommand = null;
                else
                    AcceptCommand = AcceptCommand = new Command(call =>
                    {
                        _synchronizedItem.SyncCommand.CommandAction.Invoke();
                    });
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AcceptCommand)));
            };
        }

        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name => _synchronizedItem.Name;

        /// <summary>
        /// Полный руть к отслеживаемому элементу, который представляет данная модель.
        /// </summary>
        public string FullPath => _synchronizedItem.Name;

        /// <summary>
        /// True - элемент является директорией.
        /// </summary>
        public bool IsDirectory => _synchronizedItem.IsDirectory;

        /// <summary>
        /// Выполняемая команда синхронизации. 
        /// </summary>
        public ICommand AcceptCommand { get; private set; }

        /// <summary>
        /// Статус элемента.
        /// </summary>
        public ItemStatus Status => _synchronizedItem.Status;

        /// <summary>
        /// Отображаемый моделью элемент синхронизации.
        /// </summary>
        public IItem Item => _synchronizedItem.Item;

        /// <summary>
        /// Отображаемая моделью директория. Если модель отображает файл, то null.
        /// </summary>
        public IDirectory Directory => _synchronizedItem.Directory;

        /// <summary>
        /// Путь к иконке отслеживаемого элемента.
        /// </summary>
        public string IconPath => IsDirectory ? _folderIconPath : _fileIconPath;

        /// <summary>
        /// Была изменена команда принятия элемента.
        /// </summary>
        public event Action AcceptCommandChangedEvent;

        /// <summary>
        /// Событие изменения одного из свойств модели.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Событие запуска синхронизации.
        /// </summary>
        public event Action StartedSyncEvent;

        /// <summary>
        /// Событие завершения синхронизации. Передаётся модель представления принятого элемента.
        /// </summary>
        public event Action<IItemViewModel> FinishedSyncEvent;

        /// <summary>
        /// Событие, сообщающее о завершении копирования. Передаёт копируемый элемент и элемент, в который осуществлялось копирование.
        /// </summary>
        public event Action<IItemViewModel, IItemViewModel> CopiedFromToEvent;

        /// <summary>
        /// Событие возникновения ошибки в процессе синхронизации.
        /// </summary>
        public event Action<string> SyncErrorEvent;
    }
}