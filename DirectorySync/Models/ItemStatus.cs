using System.Collections.Generic;

namespace DirectorySync.Models
{
    public class ItemStatus
    {
        private readonly static Dictionary<ItemStatusEnum, string> _iconPathesOnStatus = new Dictionary<ItemStatusEnum, string>
        {
            { ItemStatusEnum.Equally, "/DirectorySync;component/Icons/Status/Equally.png" },
            { ItemStatusEnum.Missing, "/DirectorySync;component/Icons/Status/Missing.png" },
            { ItemStatusEnum.Newer, "/DirectorySync;component/Icons/Status/Newer.png" },
            { ItemStatusEnum.Older, "/DirectorySync;component/Icons/Status/Older.png" },
            { ItemStatusEnum.ThereIs, "/DirectorySync;component/Icons/Status/ThereIs.png" },
            { ItemStatusEnum.Unknown, "/DirectorySync;component/Icons/Status/Unknown.png" }
        };

        public readonly ItemStatusEnum StatusEnum;

        public string IconPath => _iconPathesOnStatus[StatusEnum];

        public ItemStatus(ItemStatusEnum statusEnum)
        {
            StatusEnum = statusEnum;
        }
    }
}