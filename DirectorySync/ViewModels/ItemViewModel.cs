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

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="name">Наименование отображаемого элемента.</param>
        /// <param name="isDirectory">True - присутствующий элемент является директорией.</param>
        /// <param name="item">Отслеживаемый элемент, на основе которого создаётся модель.</param>
        public ItemViewModel(string fullPath, bool isDirectory, IItem item)
        {
            Name = System.IO.Path.GetFileName(fullPath);
            FullPath = fullPath;
            IsDirectory = isDirectory;
            Item = item;
            if (item != null)
            {
                item.DeletedEvent += DeletedItem;
                item.SyncErrorEvent += (string error) => { Status.Comment = error; };
                item.CopiedFromToEvent += CopiedItemTo;
                if (item is IDirectory)
                {
                    Directory = (IDirectory)item;
                    Directory.LoadedDirectoryEvent += LoadedDirectory;
                }
            }
        }

        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Полный руть к отслеживаемому элементу, который представляет данная модель.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// True - элемент является директорией.
        /// </summary>
        public bool IsDirectory { get; }

        /// <summary>
        /// Выполняемая команда синхронизации. 
        /// </summary>
        public ICommand AcceptCommand { get; private set; }

        /// <summary>
        /// Статус элемента.
        /// </summary>
        public ItemStatus Status { get; private set; }

        /// <summary>
        /// Отображаемый моделью элемент синхронизации.
        /// </summary>
        public IItem Item { get; private set; }

        /// <summary>
        /// Отображаемая моделью директория. Если модель отображает файл, то null.
        /// </summary>
        public IDirectory Directory { get; }

        /// <summary>
        /// Путь к иконке отслеживаемого элемента.
        /// </summary>
        public string IconPath => IsDirectory ? _folderIconPath : _fileIconPath;

        /// <summary>
        /// Действия команды синхронизации.
        /// </summary>
        public Func<Task> CommandAction { get; private set; }

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
        /// Обновление статуса.
        /// </summary>
        /// <param name="statusEnum">Новое значение статуса.</param>
        public void UpdateStatus(ItemStatusEnum statusEnum)
        {
            if (Status == null || Status.StatusEnum != statusEnum)
            {
                Status = new ItemStatus(statusEnum);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        /// <summary>
        /// Задание метода, который будет выполняться как команда синхронизации.
        /// </summary>
        /// <param name="action">Метод для синхронизации.</param>
        public void SetActionCommand(Func<Task> action)
        {
            if (CommandAction != action)
            {
                CommandAction = action;
                if (action == null)
                    AcceptCommand = null;
                else
                {
                    AcceptCommand = new Command(call =>
                    {
                        Task.Run(() =>
                        {
                            StartedSyncEvent?.Invoke();
                            action.Invoke().Wait();
                            FinishedSyncEvent?.Invoke(this);
                        });
                    });
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AcceptCommand)));
                AcceptCommandChangedEvent?.Invoke();
            }
        }

        private void LoadedDirectory(IDirectory directory)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
        }

        private void DeletedItem()
        {
            Item = null;
        }

        private void CopiedItemTo(IItem toItem, string destinationPath)
        {
            CopiedFromToEvent?.Invoke(this, new ItemViewModel(destinationPath, IsDirectory, toItem));
        }
    }
}