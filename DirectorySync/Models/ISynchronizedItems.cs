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
        /// True - элементы синхронизации находятся в процессе загрузки или синхронизации.
        /// </summary>
        bool InProcess { get; }

        /// <summary>
        /// Событие начала загрузки отслеживаемых директорий.
        /// </summary>
        event Action StartLoadDirectoriesEvent;

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
        /// Событие оповещает, что признак InProcess был изменён и передаёт новое значение.
        /// </summary>
        event Action<bool> InProcessChangedEvent;

        /// <summary>
        /// Загрузка директорий.
        /// </summary>
        Task Load();

        /// <summary>
        /// Загрузка дочерних записей.
        /// </summary>
        void LoadChildItems();

        /// <summary>
        /// Оповещение об удалении элемента.
        /// </summary>
        void IsDeleted();

        /// <summary>
        /// Поменять значение признака InProcess у текущих элементов и у всех дочерних.
        /// </summary>
        void InProcessChange(bool inProcess);
    }
}