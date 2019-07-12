namespace DirectorySync.ViewModels
{
    public interface ISynchronizedItemsViewModel
    {
        IItemViewModel LeftItem { get; }

        IItemViewModel RightItem { get; }

        bool Collapsed { get; set; }

        ISynchronizedItemsViewModel[] ChildItems { get; set; }
    }
}