namespace DirectorySync.ViewModels
{
    public interface ISynchronizedDataRow
    {
        ISynchronizedDirectoryViewModel LeftDirectory { get; }

        ISynchronizedDirectoryViewModel RightDirectory { get; }
    }
}