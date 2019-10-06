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
        private const string fileIconPath = "/DirectorySync;component/Icons/File.png";
        private const string folderIconPath = "/DirectorySync;component/Icons/Folder.png";

        private readonly IItem _item;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="item">Отслеживаемый элемент, на основе которого создаётся модель.</param>
        public ItemViewModel(IItem item)
        {
            _item = item;
            Name = item.Name;
            if (item is IDirectory)
            {
                Directory = (IDirectory)item;
                Directory.LoadedDirectoryEvent += LoadedDirectory;
            }
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="item">Отслеживаемый элемент, на основе которого создаётся модель.</param>
        /// <param name="itemStatusEnum">Статус, с которым будет создана модель.</param>
        public ItemViewModel(IItem item, ItemStatusEnum itemStatusEnum) : this(item)
        {
            Status = new ItemStatus(itemStatusEnum);
        }

        /// <summary>
        /// Конструктор создания отсутствующего элемента.
        /// </summary>
        /// <param name="name">Наименование отображаемого элемента.</param>
        public ItemViewModel(string name)
        {
            _item = null;
            Name = name;
            Status = new ItemStatus(ItemStatusEnum.Missing);
        }

        /// <summary>
        /// Наименование.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Выполняемая команда синхронизации. 
        /// </summary>
        public ICommand AcceptCommand => throw new NotImplementedException();

        /// <summary>
        /// Статус элемента.
        /// </summary>
        public ItemStatus Status { get; private set; }

        /// <summary>
        /// Отображаемая моделью директория. Если модель отображает файл, то null.
        /// </summary>
        public IDirectory Directory { get; }

        public string IconPath => Directory == null ? fileIconPath : folderIconPath;

        /// <summary>
        /// Событие изменения одного из свойств модели.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void LoadedDirectory(IDirectory directory)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
        }

        /// <summary>
        /// Обновление статуса.
        /// </summary>
        /// <param name="statusEnum">Новое значение статуса.</param>
        public void UpdateStatus(ItemStatusEnum statusEnum)
        {
            if (Status == null || Status.StatusEnum != statusEnum)
                Status = new ItemStatus(statusEnum);
        }
    }
}