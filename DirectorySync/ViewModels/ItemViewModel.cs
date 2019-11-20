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
        /// <param name="synchronizedItem">Элемент синхронизации, на основе которого создаётся модель представления.</param>
        public ItemViewModel(ISynchronizedItem synchronizedItem)
        {
            _synchronizedItem = synchronizedItem;
            _synchronizedItem.SyncErrorEvent += (string message) => { SyncErrorEvent?.Invoke(message); };
            _synchronizedItem.StatusChangedEvent += () => { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status))); };
            _synchronizedItem.SyncCommand.CommandActionChangedEvent += RefreshAcceptCommand;

            RefreshAcceptCommand();
        }

        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name => _synchronizedItem.Name;

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
        /// Отображаемая моделью директория. Если модель отображает файл, то null.
        /// </summary>
        public IDirectory Directory => _synchronizedItem.Directory;

        /// <summary>
        /// Путь к иконке отслеживаемого элемента.
        /// </summary>
        public string IconPath => IsDirectory ? _folderIconPath : _fileIconPath;

        /// <summary>
        /// Событие изменения одного из свойств модели.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Событие запуска синхронизации.
        /// </summary>
        public event Action StartedSyncEvent;

        /// <summary>
        /// Событие возникновения ошибки в процессе синхронизации.
        /// </summary>
        public event Action<string> SyncErrorEvent;

        /// <summary>
        /// Событие изменения команды синхронизации.
        /// </summary>
        public event Action AcceptCommandChangedEvent;

        private void RefreshAcceptCommand()
        {
            if (_synchronizedItem.SyncCommand.CommandAction == null)
                AcceptCommand = null;
            else
                AcceptCommand = new Command(async call =>
                {
                    StartedSyncEvent?.Invoke();
                    await _synchronizedItem.SyncCommand.Process();
                });
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AcceptCommand)));
            AcceptCommandChangedEvent?.Invoke();
        }
    }
}