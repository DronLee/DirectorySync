using System.Collections.Generic;

namespace DirectorySync.Models
{
    /// <summary>
    /// Статус отслеживаемого элемента.
    /// </summary>
    public class ItemStatus
    {
        private readonly static Dictionary<ItemStatusEnum, string> _iconPathesOnStatus = new Dictionary<ItemStatusEnum, string>
        {
            { ItemStatusEnum.Equally, "/DirectorySync;component/Icons/Status/Equally.png" },
            { ItemStatusEnum.Missing, "/DirectorySync;component/Icons/Status/Missing.png" },
            { ItemStatusEnum.Newer, "/DirectorySync;component/Icons/Status/Newer.png" },
            { ItemStatusEnum.Older, "/DirectorySync;component/Icons/Status/Older.png" },
            { ItemStatusEnum.ThereIs, "/DirectorySync;component/Icons/Status/ThereIs.png" },
            { ItemStatusEnum.Unknown, "/DirectorySync;component/Icons/Status/Unknown.png" },
            { ItemStatusEnum.LoadError, "/DirectorySync;component/Icons/Status/Error.png" }
        };

        private readonly static Dictionary<ItemStatusEnum, string> _commentsOnStatus = new Dictionary<ItemStatusEnum, string>
        {
            { ItemStatusEnum.Equally, "Идентично" },
            { ItemStatusEnum.Missing, "Отсутствует" },
            { ItemStatusEnum.Newer, "Более новая дата" },
            { ItemStatusEnum.Older, "Более старая дата" },
            { ItemStatusEnum.ThereIs, "Есть" },
            { ItemStatusEnum.Unknown, "Не однозначно" }
        };

        /// <summary>
        /// Значение статуса.
        /// </summary>
        public readonly ItemStatusEnum StatusEnum;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="statusEnum">Значение статуса.</param>
        /// <param name="comment">Пояснение к статусу.</param>
        public ItemStatus(ItemStatusEnum statusEnum, string comment = null)
        {
            StatusEnum = statusEnum;
            Comment = comment ?? _commentsOnStatus[statusEnum];
        }

        /// <summary>
        /// Путь к иконке статуса.
        /// </summary>
        public string IconPath => _iconPathesOnStatus[StatusEnum];

        /// <summary>
        /// Пояснение к статусу.
        /// </summary>
        public string Comment { get; }
    }
}