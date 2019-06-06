namespace DirectorySync.Models
{
    public class ItemFactory : IItemFactory
    {
        public IDirectory CreateDirectory(string directoryPath)
        {
            return new Directory(directoryPath);
        }

        public IItem CreateFile(string filePath)
        {
            return new File(filePath);
        }
    }
}