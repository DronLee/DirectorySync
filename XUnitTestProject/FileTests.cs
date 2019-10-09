using System;
using System.IO;
using Xunit;

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
                var file = new DirectorySync.Models.File(sourceFile);
                file.CopyTo(destinationFile).Wait();

                Assert.True(File.Exists(sourceFile));
                Assert.True(File.Exists(destinationFile));
                Assert.Equal(fileText, File.ReadAllText(destinationFile));
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
    }
}