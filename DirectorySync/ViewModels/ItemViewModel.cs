using System;
using System.ComponentModel;
using System.Threading.Tasks;
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

        private readonly IItem _item;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="item">Отслеживаемый элемент, на основе которого создаётся модель.</param>
        public ItemViewModel(IItem item, Action acceptAction = null)
        {
            _item = item;
            _item.DeletedEvent += () => { ItemIsDeletedEvent?.Invoke(); };
            Name = item.Name;
            if (item is IDirectory)
            {
                Directory = (IDirectory)item;
                Directory.LoadedDirectoryEvent += LoadedDirectory;
                IsDirectory = true;
            }
            if (acceptAction != null)
                SetActionCommand(acceptAction);
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="item">Отслеживаемый элемент, на основе которого создаётся модель.</param>
        /// <param name="itemStatusEnum">Статус, с которым будет создана модель.</param>
        public ItemViewModel(IItem item, ItemStatusEnum itemStatusEnum, Action acceptAction)
            : this(item, acceptAction)
        {
            Status = new ItemStatus(itemStatusEnum);
        }

        /// <summary>
        /// Конструктор создания отсутствующего элемента.
        /// </summary>
        /// <param name="name">Наименование отображаемого элемента.</param>
        /// <param name="isDirectory">True - присутствующий элемент является директорией.</param>
        public ItemViewModel(string name, bool isDirectory, Action acceptAction)
        {
            _item = null;
            Name = name;
            Status = new ItemStatus(ItemStatusEnum.Missing);
            IsDirectory = isDirectory;
            SetActionCommand(acceptAction);
        }

        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// True - элемент является директорией.
        /// </summary>
        public bool IsDirectory { get; }

        /// <summary>
        /// Выполняемая команда синхронизации. 
        /// </summary>
        public ICommand AcceptCommand { get; set; }

        /// <summary>
        /// Статус элемента.
        /// </summary>
        public ItemStatus Status { get; private set; }

        /// <summary>
        /// Отображаемая моделью директория. Если модель отображает файл, то null.
        /// </summary>
        public IDirectory Directory { get; }

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
        /// Событие завершения синхронизации.
        /// </summary>
        public event Action FinishedSyncEvent;
        /// <summary>
        /// Событие возникае, после удаления элемента, на основание которого создана данная модель представления.
        /// </summary>
        public event Action ItemIsDeletedEvent;

        /// <summary>
        /// Обновление статуса.
        /// </summary>
        /// <param name="statusEnum">Новое значение статуса.</param>
        public void UpdateStatus(ItemStatusEnum statusEnum)
        {
            if (Status == null || Status.StatusEnum != statusEnum)
                Status = new ItemStatus(statusEnum);
        }

        /// <summary>
        /// Задание метода, который будет выполняться как команда синхронизации.
        /// </summary>
        /// <param name="action">Метод для синхронизации.</param>
        public void SetActionCommand(Action action)
        {
            AcceptCommand = new Command(call =>
            {
                Task.Run(() =>
                {
                    StartedSyncEvent?.Invoke();
                    action.Invoke();
                    FinishedSyncEvent?.Invoke();
                });
            });
        }

        private void LoadedDirectory(IDirectory directory)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
        }
    }
}