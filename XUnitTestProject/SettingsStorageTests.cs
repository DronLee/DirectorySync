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
    }
}