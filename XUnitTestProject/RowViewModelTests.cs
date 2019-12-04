using DirectorySync.Models;
using DirectorySync.ViewModels;
using Xunit;

namespace XUnitTestProject
{
    public class RowViewModelTests
    {
        /// <summary>
        /// Проверка свойств модели представления строки сразу после инициализации.
        /// </summary>
        /// <param name="isDirectory">True - создаётся строка на директорию.
        /// False - создаётся строка на файл.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Init(bool isDirectory)
        {
            var leftSynchronizedItem = new SynchronizedItem("LeftItem", isDirectory, null);
            var rightSynchronizedItem = new SynchronizedItem("RightItem", isDirectory, null);
            var leftItemViewModel = new ItemViewModel(leftSynchronizedItem);
            var rightItemViewModel = new ItemViewModel(rightSynchronizedItem);

            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);

            Assert.Equal(leftItemViewModel, rowViewModel.LeftItem);
            Assert.Equal(rightItemViewModel, rowViewModel.RightItem);

            Assert.Equal(isDirectory, rowViewModel.IsDirectory);

            Assert.Empty(rowViewModel.ChildRows); // Пока дочерние строки не добавлялись.

            Assert.False(rowViewModel.InProcess);

            // Пока команды синхронизации не заданы, кнопки команд должны быть не видиммы.
            Assert.False(rowViewModel.CommandButtonIsVisible);
        }
    }
}