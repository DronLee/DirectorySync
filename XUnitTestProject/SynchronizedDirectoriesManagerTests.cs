using DirectorySync.Models;
using DirectorySync.Models.Settings;
using System;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject
{
    public class SynchronizedDirectoriesManagerTests
    {
        [Fact]
        public void Init()
        {
            var testItemFactory = new TestItemFactory();

            var testSettingsStorage = new TestSettingsStorage();
            testSettingsStorage.SettingsRows = new[]
            {
                new SettingsRow("1", "2", false),
                new SettingsRow("3", "4", true)
            };

            var synchronizedDirectoriesManager = new SynchronizedDirectoriesManager(testSettingsStorage, testItemFactory);

            Assert.Single(synchronizedDirectoriesManager.SynchronizedDirectories);
            Assert.Single(synchronizedDirectoriesManager.LeftDirectories);
            Assert.Single(synchronizedDirectoriesManager.RightDirectories);
            var synchronizedDirectory = synchronizedDirectoriesManager.SynchronizedDirectories[0];
            Assert.Equal(synchronizedDirectoriesManager.LeftDirectories[0], synchronizedDirectory.LeftDirectory);
            Assert.Equal(synchronizedDirectoriesManager.RightDirectories[0], synchronizedDirectory.RightDirectory);
            Assert.Equal("3", synchronizedDirectory.LeftDirectory.FullPath);
            Assert.Equal("4", synchronizedDirectory.RightDirectory.FullPath);
        }

        private class TestItemFactory : IItemFactory
        {
            public IDirectory CreateDirectory(string directoryPath)
            {
                return new TestDirectoryModel(directoryPath);
            }

            public IItem CreateFile(string filePath)
            {
                throw new NotImplementedException();
            }
        }

        private class TestDirectoryModel : IDirectory
        {
            public TestDirectoryModel(string directoryPath)
            {
                FullPath = directoryPath;
            }

            public IItem[] Items => throw new NotImplementedException();

            public bool IsLoaded => throw new NotImplementedException();

            public string Name => throw new NotImplementedException();

            public string FullPath { get; private set; }

            public DateTime LastUpdate => throw new NotImplementedException();

            public event LoadedDirectory LoadedDirectoryEvent;

            public Task CopyTo(string destinationPath)
            {
                throw new NotImplementedException();
            }

            public Task Delete()
            {
                throw new NotImplementedException();
            }

            public Task Load()
            {
                throw new NotImplementedException();
            }
        }

        private class TestSettingsStorage : ISettingsStorage
        {
            public ISettingsRow[] SettingsRows { get; set; }

            public ISettingsRow CreateSettingsRow(string leftDirectoryPath, string rightDirectoryPath, bool isUsed)
            {
                throw new NotImplementedException();
            }

            public void Save()
            {
                throw new NotImplementedException();
            }
        }
    }
}
