namespace DirectorySync.Models
{
    public class ItemFactory : IItemFactory
    {
        public IDirectory CreateDirectory(string directoryPath)
        {
            return new Directory(directoryPath, this);
        }

        public IItem CreateFile(string filePath)
        {
            return new File(filePath);
        }
    }
}