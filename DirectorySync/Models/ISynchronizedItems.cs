using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DirectorySync.Models
{
    /// <summary>
    /// Интерфейс описывает пару синхронизируемых элементов.
    /// </summary>
    public interface ISynchronizedItems
    {
        /// <summary>
        /// Директории загружены.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Левая директория.
        /// </summary>
        IDirectory LeftDirectory { get; }

        /// <summary>
        /// Правая директория.
        /// </summary>
        IDirectory RightDirectory { get; }

        /// <summary>
        /// Левый элемент.
        /// </summary>
        ISynchronizedItem LeftItem { get; }

        /// <summary>
        /// Правый элемент.
        /// </summary>
        ISynchronizedItem RightItem { get; }

        /// <summary>
        /// Дочерние пары синхронизируемых элементов.
        /// </summary>
        List<ISynchronizedItems> ChildItems { get; }

        /// <summary>
        /// Событие, возникающее при полной загрузке обоих директорий. Передаётся текущая модель.
        /// </summary>
        event Action<ISynchronizedItems> DirectoriesIsLoadedEvent;

        /// <summary>
        /// Событие оповещает, что пара синхронизируемых элементов удаляется и передаёт запись на них.
        /// </summary>
        event Action<ISynchronizedItems> DeleteEvent;

        /// <summary>
        /// Событие оповещает, что пара синхронизируемых элементов удалена.
        /// </summary>
        event Action DeletedEvent;

        /// <summary>
        /// Загрузка директорий.
        /// </summary>
        Task Load();

        /// <summary>
        /// Пометка о том, что требуется загрузка для данной пары синхронизируемых директорий.
        /// </summary>
        void LoadRequired();

        /// <summary>
        /// Загрузка дочерних записей.
        /// </summary>
        void LoadChildItems();

        /// <summary>
        /// Оповещение об удалении элемента.
        /// </summary>
        void IsDeleted();
    }
}