using DirectorySync.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using XUnitTestProject.Infrastructure;
using IODirectory = System.IO.Directory;
using IO = System.IO;

namespace XUnitTestProject
{
    public class DirectoryTests
    {
        /// <summary>
        /// Тестирование создания объекта директории.
        /// </summary>
        /// <param name="directoryName">Наименование создаваемой директории.</param>
        /// <param name="strLastWriteTime">Дата последнего обновления создаваемой директории.</param>
        [Theory]
        [InlineData("Directory1", "2019-01-01 06:00:00")]
        [InlineData("Directory2", "2018-01-01 16:30:20")]
        public void Constructor(string directoryName, string strLastWriteTime)
        {
            var lastWriteTime = DateTime.Parse(strLastWriteTime);
            using (var testDirectory = new TestDirectory())
            {
                var directoryPath = testDirectory.CreateDirectory(directoryName, lastWriteTime);
                var directory = new Directory(directoryPath, new TestItemFactory());

                Assert.Equal(directoryName, directory.Name);
                Assert.Equal(directoryPath, directory.FullPath);
                Assert.Equal(lastWriteTime, directory.LastUpdate);
                Assert.False(directory.IsLoaded);
                Assert.Empty(directory.Items); // Загрузка не выполнялась, содержимого быть не должно.
            }
        }

        [Fact]
        public async Task CopyTo()
        {
            using (var testDirectory = new TestDirectory())
            {
                var sourceDirectory =  testDirectory.CreateDirectory("SorceDir");
                var destinationDirectory = System.IO.Path.Combine(testDirectory.FullPath, "DestDir");

                IItem destinationCopyDirectory = null;
                string destinationCopyPath = null;

                var directory = new Directory(sourceDirectory, new TestItemFactory());
                directory.CopiedFromToEvent += (IItem destinationItem, string destinationPath) =>
                {
                    destinationCopyDirectory = destinationItem;
                    destinationCopyPath = destinationPath;
                };
                await directory.CopyTo(destinationDirectory);

                Assert.True(IODirectory.Exists(sourceDirectory));
                Assert.True(IODirectory.Exists(destinationDirectory));
                Assert.NotNull(destinationCopyDirectory);
                Assert.Equal(destinationDirectory, destinationCopyDirectory.FullPath);
                Assert.Equal(destinationDirectory, destinationCopyPath);
            }
        }

        [Fact]
        public async Task Delete()
        {
            var deletedEvent = false;

            using (var testDirectory = new TestDirectory())
            {
                var sourceDirectory = testDirectory.CreateDirectory("SorceDir");
                var directory = new Directory(sourceDirectory, new TestItemFactory());
                directory.DeletedEvent += () => { deletedEvent = true; };

                await directory.Delete();

                Assert.True(deletedEvent);
                Assert.False(IODirectory.Exists(sourceDirectory));
            }
        }

        /// <summary>
        /// Тестирование на получение ошибки в процессе удаления.
        /// </summary>
        [Fact]
        public async Task DeleteWithError()
        {
            using (var testDirectory = new TestDirectory())
            {
                var sourceDirectory = testDirectory.CreateDirectory("SorceDir");
                var directory = new Directory(sourceDirectory, new TestItemFactory());

                string error = null;
                directory.SyncErrorEvent += (string message) => { error = message; };

                using (var stream = IO.File.Create(IO.Path.Combine(sourceDirectory, "1")))
                    await directory.Delete();

                Assert.NotNull(error);
            }
        }

        /// <summary>
        /// Тестирование загрузки директории.
        /// </summary>
        [Fact]
        public async Task Load()
        {
            using (var testDirectory = new TestDirectory())
            {
                var directory = new Directory(testDirectory.FullPath, new TestItemFactory());

                const string emptyDirectoryName = "EmptyDirectory";
                var emptyDirectoryLastUpdate = new DateTime(2019, 2, 2, 15, 30, 20);
                var emptyDirectoryPath = testDirectory.CreateDirectory(emptyDirectoryName, emptyDirectoryLastUpdate);

                const string notEmptyDirectoryName = "NotEmptyDirectory";
                var notEmptyDirectoryLastUpdate = new DateTime(2019, 4, 4, 16, 35, 25);
                var notEmptyDirectoryPath = testDirectory.CreateDirectory(notEmptyDirectoryName);

                const string file1Name = "File1";
                var file1LastUpdate = new DateTime(2018, 3, 3, 12, 0, 30);
                const string file2Name = "File2";
                var file2LastUpdate = new DateTime(2019, 2, 3, 11, 5, 10);
                TestDirectory.CreateFiles(notEmptyDirectoryPath, new Dictionary<string, DateTime>
                {
                    { file1Name, file1LastUpdate },
                    { file2Name,  file2LastUpdate}
                });

                // Только после добавления файлов в директорию, так как дата перетёрлась бы.
                IODirectory.SetLastWriteTime(notEmptyDirectoryPath, notEmptyDirectoryLastUpdate);

                const string rootFileName = "RootFile";
                var rootFileLastUpdate = new DateTime(2019, 2, 5, 8, 5, 0);
                testDirectory.CreateFiles(new Dictionary<string, DateTime> { { rootFileName, rootFileLastUpdate } });

                await directory.Load();

                Assert.True(directory.IsLoaded);
                Assert.Equal(3, directory.Items.Length); // Один файл и две директории.

                // Сначала идут директории, а потом файлы.
                Assert.IsType<Directory>(directory.Items[0]);
                Assert.IsType<Directory>(directory.Items[1]);
                Assert.IsType<TestFile>(directory.Items[2]);

                Assert.Equal(emptyDirectoryName, directory.Items[0].Name);
                Assert.Equal(emptyDirectoryPath, directory.Items[0].FullPath);
                Assert.Equal(emptyDirectoryLastUpdate, directory.Items[0].LastUpdate);
                Assert.True(((IDirectory)directory.Items[0]).IsLoaded);
                Assert.Empty(((IDirectory)directory.Items[0]).Items);

                Assert.Equal(notEmptyDirectoryName, directory.Items[1].Name);
                Assert.Equal(notEmptyDirectoryPath, directory.Items[1].FullPath);
                Assert.Equal(notEmptyDirectoryLastUpdate, directory.Items[1].LastUpdate);
                Assert.True(((IDirectory)directory.Items[1]).IsLoaded);
                Assert.Equal(2, ((IDirectory)directory.Items[1]).Items.Length);
                var file1 = ((IDirectory)directory.Items[1]).Items[0];
                Assert.Equal(file1Name, file1.Name);
                Assert.Equal(file1LastUpdate, file1.LastUpdate);
                var file2 = ((IDirectory)directory.Items[1]).Items[1];
                Assert.Equal(file2Name, file2.Name);
                Assert.Equal(file2LastUpdate, file2.LastUpdate);

                Assert.Equal(rootFileName, directory.Items[2].Name);
                Assert.Equal(rootFileLastUpdate, directory.Items[2].LastUpdate);
            }
        }

        private class TestItemFactory : IItemFactory
        {
            public IDirectory CreateDirectory(string directoryPath)
            {
                return new Directory(directoryPath, this);
            }

            public IItem CreateFile(string filePath)
            {
                return new TestFile(filePath);
            }
        }

        private class TestFile : IItem
        {
            public TestFile(string path)
            {
                var fileInfo = new System.IO.FileInfo(path);
                FullPath = path;
                Name = fileInfo.Name;
                LastUpdate = fileInfo.LastWriteTime;
            }

            public string Name { get; }

            public string FullPath { get; }

            public DateTime LastUpdate { get; }

            public event Action DeletedEvent;
            public event Action<string> SyncErrorEvent;
            public event Action<IItem, string> CopiedFromToEvent;

            public Task CopyTo(string destinationPath)
            {
                throw new NotImplementedException();
            }

            public Task Delete()
            {
                throw new NotImplementedException();
            }
        }
    }
}