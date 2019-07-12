using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace DirectorySync.Models
{
    public class SynchronizedDirectoriesManager : ISynchronizedDirectoriesManager
    {
        private readonly List<ISynchronizedDirectories> _synchronizedDirectoriesList;

        public SynchronizedDirectoriesManager(StringCollection leftDirectories, StringCollection rightDirectories, IItemFactory itemFactory)
        {
            if (leftDirectories.Count != rightDirectories.Count)
                throw new Exception("Количество левых и правых директорий должно совпадать.");

            _synchronizedDirectoriesList = new List<ISynchronizedDirectories>();
            for (int i = 0; i < leftDirectories.Count; i++)
                _synchronizedDirectoriesList.Add(new SynchronizedDirectories(leftDirectories[i], rightDirectories[i], itemFactory));
        }

        public IDirectory[] LeftDirectories => _synchronizedDirectoriesList.Select(d => d.LeftDirectory).ToArray();

        public IDirectory[] RightDirectories => _synchronizedDirectoriesList.Select(d => d.RightDirectory).ToArray();

        public ISynchronizedDirectories[] SynchronizedDirectories => _synchronizedDirectoriesList.ToArray();

        public async Task Load()
        {
            foreach (var synchronizedDirectories in _synchronizedDirectoriesList)
                await synchronizedDirectories.Load();
        }
    }
}