namespace DirectorySync.Models
{
    public interface IItemFactory
    {
        IDirectory CreateDirectory(string directoryPath);

        IItem CreateFile(string filePath);
    }
}