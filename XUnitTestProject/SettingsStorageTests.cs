using DirectorySync.Models.Settings;
using Newtonsoft.Json;
using System.IO;
using System.Text;
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

                // Каждый раз при инициализации хранилище должно проверять какие директории есть, а каких нет.
                foreach(var row in storage.SettingsRows)
                {
                    Assert.Equal(!Directory.Exists(row.LeftDirectory.DirectoryPath), row.LeftDirectory.NotFound);
                    Assert.Equal(!Directory.Exists(row.RightDirectory.DirectoryPath), row.RightDirectory.NotFound);
                }
            }
        }

        [Theory]
        [InlineData("C:\\1", "D:\\2", true, null)]
        [InlineData("C:\\2", "D:\\1", false, new[] { "tiff", "png" })]
        public void CreateSettingsRow(string leftDirectory, string rightDirectory, bool isUsed, string[] excludedExtensions)
        {
            var storage = new SettingsStorage("C:\\Test");
            var result = storage.CreateSettingsRow(leftDirectory, rightDirectory, isUsed, excludedExtensions);

            Assert.Equal(leftDirectory, result.LeftDirectory.DirectoryPath);
            Assert.Equal(rightDirectory, result.RightDirectory.DirectoryPath);
            Assert.Equal(isUsed, result.IsUsed);
            Assert.Equal(excludedExtensions, result.ExcludedExtensions);

            //При создании не проверяется наличие директории, создаётся, как будто существующая.
            Assert.False(result.LeftDirectory.NotFound);
            Assert.False(result.RightDirectory.NotFound);
        }

        [Fact]
        public void Save()
        {
            using (var testDirectory = new TestDirectory())
            {
                var settingsFile = Path.Combine(testDirectory.FullPath, "TestSettings");
                var storage = new SettingsStorage(settingsFile);
                storage.SettingsRows = new[]
                {
                    new SettingsRow("1", "2", false, new[] { "jpg", "png" }),
                    new SettingsRow("3", "4", true, null)
                };

                // Чтобы потом проверить, что при записи NotFound не сохранилось.
                foreach (var row in storage.SettingsRows)
                {
                    row.LeftDirectory.NotFound = true;
                    row.RightDirectory.NotFound = true;
                }

                storage.Save();

                Assert.True(File.Exists(settingsFile));

                var settingsFileData = JsonConvert.DeserializeObject<SettingsRow[]>(
                    File.ReadAllText(settingsFile, Encoding.UTF8));
                Assert.Equal(storage.SettingsRows.Length, settingsFileData.Length);
                for(int i=0; i< storage.SettingsRows.Length; i++)
                {
                    var expectedRow = storage.SettingsRows[i];
                    var resultRow = settingsFileData[i];

                    Assert.Equal(expectedRow.LeftDirectory.DirectoryPath, resultRow.LeftDirectory.DirectoryPath);
                    Assert.Equal(expectedRow.RightDirectory.DirectoryPath, resultRow.RightDirectory.DirectoryPath);
                    Assert.Equal(expectedRow.IsUsed, resultRow.IsUsed);
                    Assert.Equal(expectedRow.ExcludedExtensions, resultRow.ExcludedExtensions);

                    // NotFound не хранится в файле, так как каждый раз при загрузке снова проверяется.
                    Assert.False(resultRow.LeftDirectory.NotFound);
                    Assert.False(resultRow.RightDirectory.NotFound);
                }
            }
        }
    }
}