using DirectorySync.Models.Settings;
using System.IO;
using Xunit;
using XUnitTestProject.Infrastructure;

namespace XUnitTestProject
{
    public class SettingsStorageTests
    {
        [Theory]
        [InlineData("NotExists", 0)]
        [InlineData("2RowsSettings", 2)]
        public void Init(string settingsFileName, byte rowsCount)
        {
            using (var testDirectory = new TestDirectory())
            {
                var settingsFilePath = Path.Combine(testDirectory.FullPath, settingsFileName);
                foreach (string file in Directory.GetFiles(TestFilesManager.SettingsDirectory, settingsFileName))
                    File.Copy(file, settingsFilePath);

                var storage = new SettingsStorage(settingsFilePath);

                Assert.Equal(rowsCount, storage.SettingsRows.Length);
            }
        }

        [Theory]
        [InlineData("C:\\1", "D:\\2", true)]
        [InlineData("C:\\2", "D:\\1", false)]
        public void CreateSettingsRow(string leftDirectory, string rightDirectory, bool isUsed)
        {
            var storage = new SettingsStorage("C:\\Test");
            var result = storage.CreateSettingsRow(leftDirectory, rightDirectory, isUsed);

            Assert.Equal(leftDirectory, result.LeftDirectory.DirectoryPath);
            Assert.Equal(rightDirectory, result.RightDirectory.DirectoryPath);
            Assert.Equal(isUsed, result.IsUsed);

            //При создании не проверяется наличие директории, создаётся, как будто существующая.
            Assert.False(result.LeftDirectory.NotFound);
            Assert.False(result.RightDirectory.NotFound);
        }
    }
}