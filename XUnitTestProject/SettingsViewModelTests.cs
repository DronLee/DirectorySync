using DirectorySync.Models.Settings;
using DirectorySync.ViewModels;
using DirectorySync.ViewModels.Settings;
using System;
using System.Linq;
using Xunit;

namespace XUnitTestProject
{
    public class SettingsViewModelTests
    {
        [Fact]
        public void Init()
        {
            var settingsStorage = new TestSettingsStorage();
            settingsStorage.SettingsRows = new[]
            {
                new SettingsRow("1", null, true, new string[0]),
                new SettingsRow(null, "2", false, new[] { "jpg", "png" })
            };

            // Чтобы потом проверить и NotFound.
            settingsStorage.SettingsRows[0].RightDirectory.NotFound = true;
            settingsStorage.SettingsRows[1].LeftDirectory.NotFound = true;

            var settingsViewModel = new SettingsViewModel(settingsStorage);

            Assert.Equal(MessageTypeEnum.Default, settingsViewModel.CommentType);
            Assert.False(settingsViewModel.Ok);

            // Последняя строка в модели представления должна быть пустой, остальные - нет.
            Assert.Equal(settingsStorage.SettingsRows.Length + 1, settingsViewModel.SettingsRows.Count);
            var emptyRow = settingsViewModel.SettingsRows.Last();
            Assert.True(emptyRow.IsEmpty);
            Assert.Equal("EmptyDirectoryButton", emptyRow.LeftDirectory.ButtonStyle);
            Assert.Equal("EmptyDirectoryButton", emptyRow.RightDirectory.ButtonStyle);
            for (int i = 0; i < settingsStorage.SettingsRows.Length; i++)
                Assert.False(settingsViewModel.SettingsRows[i].IsEmpty);

            for (int i=0; i< settingsStorage.SettingsRows.Length;i++)
            {
                var settingsRow = settingsStorage.SettingsRows[i];
                var settingsRowViewModel = settingsViewModel.SettingsRows[i];
                Assert.Equal(settingsRow.IsUsed, settingsRowViewModel.IsUsed);
                Assert.Equal(settingsRow.LeftDirectory.DirectoryPath, settingsRowViewModel.LeftDirectory.DirectoryPath);
                Assert.Equal(settingsRow.LeftDirectory.NotFound, settingsRowViewModel.LeftDirectory.NotFound);
                Assert.Equal(settingsRow.RightDirectory.DirectoryPath, settingsRowViewModel.RightDirectory.DirectoryPath);
                Assert.Equal(settingsRow.RightDirectory.NotFound, settingsRowViewModel.RightDirectory.NotFound);
                Assert.Equal(string.Join(";", settingsRow.ExcludedExtensions), settingsRowViewModel.ExcludedExtensions);
            }

            // И проверка наименований стилей исходня из обнаружения дирекорий.
            Assert.Equal("LeftDirectoryButton", settingsViewModel.SettingsRows[0].LeftDirectory.ButtonStyle);
            Assert.Equal("NotFoundRightDirectoryButton", settingsViewModel.SettingsRows[0].RightDirectory.ButtonStyle);
            Assert.Equal("NotFoundLeftDirectoryButton", settingsViewModel.SettingsRows[1].LeftDirectory.ButtonStyle);
            Assert.Equal("RightDirectoryButton", settingsViewModel.SettingsRows[1].RightDirectory.ButtonStyle);
        }
        private class TestSettingsStorage : ISettingsStorage
        {
            public ISettingsRow[] SettingsRows { get; set; }

            public ISettingsRow CreateSettingsRow(string leftDirectoryPath, string rightDirectoryPath, bool isUsed, string[] excludedExtensions)
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