namespace DirectorySync.Models
{
    public class ItemStatus
    {
        public readonly ItemStatusEnum StatusEnum;
        public readonly string IconPath;

        public ItemStatus(ItemStatusEnum statusEnum)
        {
            StatusEnum = statusEnum;
        }
    }
}