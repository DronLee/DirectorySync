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
                new SettingsRow("1", null, true),
                new SettingsRow(null, "2", false)
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
            }

            // И проверка наименований стилей исходня из обнаружения дирекорий.
            Assert.Equal("LeftDirectoryButton", settingsViewModel.SettingsRows[0].LeftDirectory.ButtonStyle);
            Assert.Equal("NotFoundRightDirectoryButton", settingsViewModel.SettingsRows[0].RightDirectory.ButtonStyle);
            Assert.Equal("NotFoundLeftDirectoryButton", settingsViewModel.SettingsRows[1].LeftDirectory.ButtonStyle);
            Assert.Equal("RightDirectoryButton", settingsViewModel.SettingsRows[1].RightDirectory.ButtonStyle);
        }

        [Theory]
        [InlineData(true, "1", false, null, false, "Warning", 0, 0, false)]
        [InlineData(true, null, false, "2", false, "Warning", 0, 0, false)]
        [InlineData(true, "1", true, "2", false, "Warning", 0, 0, false)]
        [InlineData(true, "1", false, "2", true, "Warning", 0, 0, false)]
        [InlineData(true, "1", true, "2", true, "Warning", 0, 0, false)]
        [InlineData(true, "1", false, "2", false, "Default", 1, 1, true)]
        [InlineData(false, "1", false, null, false, "Default", 0, 1, true)]
        [InlineData(false, null, false, "2", false, "Default", 0, 1, true)]
        [InlineData(false, "1", true, "2", false, "Default", 0, 1, true)]
        [InlineData(false, "1", false, "2", true, "Default", 0, 1, true)]
        [InlineData(false, "1", true, "2", true, "Default", 0, 1, true)]
        [InlineData(false, "1", false, "2", false, "Default", 0, 1, true)]
        public void OkCommand(bool rowIsUsed, string leftDirectory, bool leftDirectoryIsNotFound, string rightDirectory,
            bool rightDirectoryIsNotFound, string commentTypeText, byte useCreateSettingsRowCount, byte useSaveCount, bool ok)
        {
            var settingsStorage = new TestSettingsStorage();

            var settingsRow = new SettingsRow(leftDirectory, rightDirectory, rowIsUsed);
            settingsStorage.SettingsRows = new[] { settingsRow };
            settingsRow.LeftDirectory.NotFound = leftDirectoryIsNotFound;
            settingsRow.RightDirectory.NotFound = rightDirectoryIsNotFound;

            var settingsViewModel = new SettingsViewModel(settingsStorage);

            settingsViewModel.OkCommand.Execute(null);

            Assert.Equal(Enum.Parse<MessageTypeEnum>(commentTypeText), settingsViewModel.CommentType);
            Assert.Equal(useCreateSettingsRowCount, settingsStorage.useCreateSettingsRowCount);
            Assert.Equal(useSaveCount, settingsStorage.useSaveCount);
            Assert.Equal(ok, settingsViewModel.Ok);
        }
        private class TestSettingsStorage : ISettingsStorage
        {
            public byte useCreateSettingsRowCount = 0;
            public byte useSaveCount = 0;

            public ISettingsRow[] SettingsRows { get; set; }

            public ISettingsRow CreateSettingsRow(string leftDirectoryPath, string rightDirectoryPath, bool isUsed)
            {
                useCreateSettingsRowCount++;
                return null;
            }

            public void Save()
            {
                useSaveCount++;
            }
        }
    }
}