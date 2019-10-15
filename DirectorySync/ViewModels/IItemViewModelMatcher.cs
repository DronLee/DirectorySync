namespace DirectorySync.ViewModels
{
    public interface IItemViewModelMatcher
    {
        void UpdateStatusesAndCommands(IItemViewModel itemViewModel1, IItemViewModel itemViewModel2);
    }
}