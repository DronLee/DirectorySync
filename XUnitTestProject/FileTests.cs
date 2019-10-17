using System;
using System.IO;
using Xunit;
using IItem = DirectorySync.Models.IItem;

namespace XUnitTestProject
{
    /// <summary>
    /// Тесты для модели файла.
    /// </summary>
    public class FileTests
    {
        [Fact]
        public void CopyTo()
        {
            using (var testDirectory = new Infrastructure.TestDirectory())
            {
                var sourceFile = Path.Combine(testDirectory.FullPath, Guid.NewGuid().ToString());
                var destinationFile = Path.Combine(testDirectory.FullPath, Guid.NewGuid().ToString());
                var fileText = Guid.NewGuid().ToString();

                File.WriteAllText(sourceFile, fileText);

                IItem destinationCopyFile = null;
                string destinationCopyPath = null;

                var file = new DirectorySync.Models.File(sourceFile);
                file.CopiedFromToEvent += (IItem destinationItem, string destinationPath) =>
                    {
                        destinationCopyFile = destinationItem;
                        destinationCopyPath = destinationPath;
                    };
                file.CopyTo(destinationFile).Wait();

                Assert.True(File.Exists(sourceFile));
                Assert.True(File.Exists(destinationFile));
                Assert.Equal(fileText, File.ReadAllText(destinationFile));
                Assert.NotNull(destinationCopyFile);
                Assert.Equal(destinationFile, destinationCopyFile.FullPath);
                Assert.Equal(destinationFile, destinationCopyPath);
            }
        }

        /// <summary>
        /// Тест на получение уведомления об ошибки в процессе копирования.
        /// </summary>
        [Fact]
        public void CopyToWithError()
        {
            using (var testDirectory = new Infrastructure.TestDirectory())
            {
                var sourceFile = Path.Combine(testDirectory.FullPath, Guid.NewGuid().ToString());
                var destinationFile = Path.Combine(testDirectory.FullPath, Guid.NewGuid().ToString());
                var fileText = Guid.NewGuid().ToString();

                File.WriteAllText(sourceFile, fileText);
                var file = new DirectorySync.Models.File(sourceFile);

                string error = null;
                file.SyncErrorEvent += (string message) => { error = message; };
                using (var stream = File.Create(destinationFile))
                    file.CopyTo(destinationFile).Wait();

                Assert.NotNull(error);
            }
        }

        [Fact]
        public void Delete()
        {
            var deletedEvent = false;
            using (var testDirectory = new Infrastructure.TestDirectory())
            {
                var sourceFile = Path.Combine(testDirectory.FullPath, Guid.NewGuid().ToString());
                File.WriteAllBytes(sourceFile, new byte[0]);
                var file = new DirectorySync.Models.File(sourceFile);
                file.DeletedEvent += () => { deletedEvent = true; };

                file.Delete().Wait();

                Assert.True(deletedEvent);
                Assert.False(File.Exists(sourceFile));
            }
        }

        [Fact]
        public void DeleteWithError()
        {
            using (var testDirectory = new Infrastructure.TestDirectory())
            {
                var sourceFile = Path.Combine(testDirectory.FullPath, Guid.NewGuid().ToString());
                File.WriteAllBytes(sourceFile, new byte[0]);
                var file = new DirectorySync.Models.File(sourceFile);

                string error = null;
                file.SyncErrorEvent += (string message) => { error = message; };

                using (var stream = File.Open(sourceFile, FileMode.Open))
                    file.Delete().Wait();

                Assert.NotNull(error);
            }
        }
    }
}