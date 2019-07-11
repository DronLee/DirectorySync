namespace DirectorySync.ViewModels
{
    public interface ISynchronizedDirectoryViewModel : ISynchronizedItemViewModel
    {
        bool Collapsed { get; set; }

        ISynchronizedItemViewModel[] Items { get; set; }
    }
}