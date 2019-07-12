namespace DirectorySync.ViewModels
{
    public class SynchronizedItemsViewModel : ISynchronizedItemsViewModel
    {
        public SynchronizedItemsViewModel(IItemViewModel leftItem, IItemViewModel rightItem)
        {
            LeftItem = leftItem;
            RightItem = rightItem;
        }

        public IItemViewModel LeftItem { get; private set; }

        public IItemViewModel RightItem { get; private set; }

        public bool Collapsed { get; set; }
        public ISynchronizedItemsViewModel[] ChildItems
        {
            get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException();
        }
    }
}