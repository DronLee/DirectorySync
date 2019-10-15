using DirectorySync.Models;
using DirectorySync.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xunit;

namespace XUnitTestProject
{
    public class RowViewModelTests
    {
        /// <summary>
        /// Проверка обновления статусов моделей представлений отслеживаемых элементов
        /// на основе статусов дочерних элементов.
        /// </summary>
        /// <param name="leftStartStatus">Начальное значение статуса левого элемента.</param>
        /// <param name="rightStartStatus">Начальное значение статуса правого элемента.</param>
        /// <param name="leftItemsStatuses">Статусы левых дочерних элементов.</param>
        /// <param name="rightItemsStatuses">Статусы правых дочерних элементов.</param>
        /// <param name="leftExpectedStatus">Ожидаемое значение статуса левого элемента после обновления.</param>
        /// <param name="rightExpectedStatus">Ожидаемое значение статуса правого элемента после обновления.</param>
        [Theory]

        // Если нет дочерних элементов, то статус должен оставаться прежним.
        [InlineData("Equally", "Equally", new string[0], new string[0], "Equally", "Equally")]

        [InlineData("Equally", "Equally", new[] { "Newer" }, new[] { "Older" }, "Newer", "Older")]
        [InlineData("Equally", "Equally", new[] { "Missing" }, new[] { "ThereIs" }, "Missing", "ThereIs")]
        [InlineData("Unknown", "Unknown", new[] { "Equally" }, new[] { "Equally" }, "Equally", "Equally")]

        // Если дочерние элементы имеют разнообразные статусы, стутус родительского будет Unknown.
        [InlineData("Equally", "Equally", new[] { "Missing", "Equally" }, new[] { "ThereIs", "Equally" }, "Missing", "ThereIs")]
        public void RefreshStatusesFromChilds_CheckStatus(string leftStartStatus, string rightStartStatus,
            string[] leftItemsStatuses, string[] rightItemsStatuses,
            string leftExpectedStatus, string rightExpectedStatus)
        {
            var leftItem = new TestItemViewModel("Left", (ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), leftStartStatus));
            var rightItem = new TestItemViewModel("Right", (ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), rightStartStatus));

            var rowViewModel = new RowViewModel(leftItem, rightItem, null);
            for (byte i = 0; i < leftItemsStatuses.Length; i++)
                rowViewModel.ChildRows.Add(new RowViewModel(
                    new TestItemViewModel("Left" + i.ToString(), (ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), leftItemsStatuses[i])),
                    new TestItemViewModel("Right" + i.ToString(), (ItemStatusEnum)Enum.Parse(typeof(ItemStatusEnum), rightItemsStatuses[i])), null));

            rowViewModel.RefreshStatusesFromChilds();

            Assert.Equal(leftExpectedStatus, rowViewModel.LeftItem.Status.StatusEnum.ToString());
            Assert.Equal(rightExpectedStatus, rowViewModel.RightItem.Status.StatusEnum.ToString());
        }

        /// <summary>
        /// Проверка возникновения события на удаление строки, при прнятии левого элемента, который отсутсвует.
        /// </summary>
        [Fact]
        public void DeleteRowEventAfterLeftAccept()
        {
            using (var testDirectory = new Infrastructure.TestDirectory())
            {
                var leftParentDirectoryPath = testDirectory.CreateDirectory("1");
                var rightParentDirectoryPath = testDirectory.CreateDirectory("1");

                var rightDirectoryPath = System.IO.Path.Combine(rightParentDirectoryPath, "2");
                System.IO.Directory.CreateDirectory(rightDirectoryPath);

                var rightDirectory = new Directory(rightDirectoryPath, null);

                var filesDictionary = new Dictionary<string, DateTime>();
                for (byte i = 0; i < filesDictionary.Count; i++)
                    filesDictionary.Add(i.ToString(), DateTime.Now);

                Infrastructure.TestDirectory.CreateFiles(rightDirectory.FullPath, filesDictionary);

                var parentRow = new RowViewModel(
                    new ItemViewModel(leftParentDirectoryPath, true, new Directory(leftParentDirectoryPath, null)),
                    new ItemViewModel(rightParentDirectoryPath, true, new Directory(rightParentDirectoryPath, null)), null);

                var leftItemViewModel = new ItemViewModel("Test", true, null);
                leftItemViewModel.SetActionCommand(async () => { await rightDirectory.Delete(); });
                var rowViewModel = new RowViewModel(leftItemViewModel,
                    new ItemViewModel(rightDirectory.FullPath, true, rightDirectory), parentRow);

                foreach (var fileName in filesDictionary.Keys)
                {
                    var rightItem = new File(System.IO.Path.Combine(rightDirectory.FullPath, fileName));
                    rowViewModel.ChildRows.Add(new RowViewModel(
                        new ItemViewModel(fileName, false, null),
                        new ItemViewModel(rightItem.FullPath, false, rightItem),
                        rowViewModel));
                }

                IRowViewModel deletedRow = null; // Удалённая строка.
                IRowViewModel parentDeletedRow = null; // Родительская строка удалённой строки.
                rowViewModel.DeleteRowViewModelEvent += (IRowViewModel deletingRow, IRowViewModel parentDeletingRow) =>
                {
                    deletedRow = deletingRow;
                    parentDeletedRow = parentDeletingRow;
                };

                rowViewModel.LeftItem.AcceptCommand.Execute(null);

                Thread.Sleep(50); // Чтобы успело выполниться событие.
                Assert.NotNull(deletedRow);
                Assert.Equal(rowViewModel, deletedRow);
                Assert.Equal(parentRow, parentDeletedRow);
            }
        }

        /// <summary>
        /// Проверка возникновения события на удаление строки, при прнятии правого элемента, который отсутсвует.
        /// </summary>
        [Fact]
        public void DeleteRowEventAfterRightAccept()
        {
            using (var testDirectory = new Infrastructure.TestDirectory())
            {
                var leftParentDirectoryPath = testDirectory.CreateDirectory("1");
                var rightParentDirectoryPath = testDirectory.CreateDirectory("1");

                var leftDirectoryPath = System.IO.Path.Combine(leftParentDirectoryPath, "2");
                System.IO.Directory.CreateDirectory(leftDirectoryPath);

                var leftDirectory = new Directory(leftDirectoryPath, null);

                var filesDictionary = new Dictionary<string, DateTime>();
                for (byte i = 0; i < filesDictionary.Count; i++)
                    filesDictionary.Add(i.ToString(), DateTime.Now);

                Infrastructure.TestDirectory.CreateFiles(leftDirectory.FullPath, filesDictionary);

                var parentRow = new RowViewModel(
                    new ItemViewModel(leftParentDirectoryPath, true, new Directory(leftParentDirectoryPath, null)),
                    new ItemViewModel(rightParentDirectoryPath, true, new Directory(rightParentDirectoryPath, null)), null);

                var rightItemViewModel = new ItemViewModel("Test", true, null);
                rightItemViewModel.SetActionCommand(async () => { await leftDirectory.Delete(); });
                var rowViewModel = new RowViewModel(
                    new ItemViewModel(leftDirectory.FullPath, true, leftDirectory),
                    rightItemViewModel, parentRow);

                foreach (var fileName in filesDictionary.Keys)
                {
                    var leftItem = new File(System.IO.Path.Combine(leftDirectory.FullPath, fileName));
                    rowViewModel.ChildRows.Add(new RowViewModel(
                        new ItemViewModel(leftItem.FullPath, false, leftItem),
                        new ItemViewModel(fileName, false, null), parentRow));
                }

                IRowViewModel deletedRow = null; // Удалённая строка.
                IRowViewModel parentDeletedRow = null; // Родительская строка удалённой строки.
                rowViewModel.DeleteRowViewModelEvent += (IRowViewModel deletingRow, IRowViewModel parentDeletingRow) =>
                {
                    deletedRow = deletingRow;
                    parentDeletedRow = parentDeletingRow;
                };

                rowViewModel.RightItem.AcceptCommand.Execute(null);

                Thread.Sleep(50); // Чтобы успело выполниться событие.
                Assert.NotNull(deletedRow);
                Assert.Equal(rowViewModel, deletedRow);
                Assert.Equal(parentRow, parentDeletedRow);
            }
        }

        /// <summary>
        /// Тест на выполнение команды принятия левого элемента.
        /// </summary>
        [Fact]
        public void LeftItemAcceptCommand()
        {
            var useAcceptCommand = false;
            var leftItemViewModel = new ItemViewModel("LeftItem", false, null);
            var rightItemViewModel = new ItemViewModel("RightItem", false, null);
            leftItemViewModel.SetActionCommand(() => { useAcceptCommand = true; });
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);
            rowViewModel.LeftItem.AcceptCommand.Execute(null);
            Thread.Sleep(15); //  Чтобы успела выполниться команда.

            Assert.True(useAcceptCommand);

            // Сразу после запуска выполнения команды должна удалиться.
            Assert.Null(rowViewModel.LeftItem.AcceptCommand);
            Assert.Null(rowViewModel.RightItem.AcceptCommand);
        }

        /// <summary>
        /// Тест на выполнение команды принятия правого элемента.
        /// </summary>
        [Fact]
        public void RightItemAcceptCommand()
        {
            var useAcceptCommand = false;
            var leftItemViewModel = new ItemViewModel("LeftItem", false, null);
            var rightItemViewModel = new ItemViewModel("RightItem", false, null);
            rightItemViewModel.SetActionCommand(() => { useAcceptCommand = true; });
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);
            rowViewModel.RightItem.AcceptCommand.Execute(null);
            Thread.Sleep(15); //  Чтобы успела выполниться команда.

            Assert.True(useAcceptCommand);

            // Сразу после запуска выполнения команды должна удалиться.
            Assert.Null(rowViewModel.LeftItem.AcceptCommand);
            Assert.Null(rowViewModel.RightItem.AcceptCommand);
        }

        /// <summary>
        /// Тест на простановку свойств видимости при инициализации.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_Init()
        {
            var leftItemViewModel = new ItemViewModel("LeftItem", false, null);
            var rightItemViewModel = new TestItemViewModel("RightItem", ItemStatusEnum.Older);
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);

            Assert.True(rowViewModel.CommandButtonIsVisible);
            Assert.False(rowViewModel.ProcessIconIsVisible);
        }

        /// <summary>
        /// Тест на простановку свойств видимости сразу после запуска принятия левого элемента.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_StartedLeftItemAccept()
        {
            // Sleep, чтобы была возможность проверить свойства в процессе выполнения синхронизации.
            var leftItemViewModel = new ItemViewModel("LeftItem", false, null);
            var rightItemViewModel = new TestItemViewModel("RightItem", ItemStatusEnum.Older);
            leftItemViewModel.SetActionCommand(() => { Thread.Sleep(70); });
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);
            rowViewModel.LeftItem.AcceptCommand.Execute(null);
            Thread.Sleep(25); //  Чтобы успели обновиться свойства.

            Assert.False(rowViewModel.CommandButtonIsVisible);
            Assert.True(rowViewModel.ProcessIconIsVisible);
        }

        /// <summary>
        /// Тест на простановку свойств видимости сразу после запуска принятия правого элемента.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_StartedRightItemAccept()
        {
            var leftItemViewModel = new TestItemViewModel("LeftItem", ItemStatusEnum.Older);
            // Sleep, чтобы была возможность проверить свойства в процессе выполнения синхронизации.
            var rightItemViewModel = new ItemViewModel("RightItem", false, null);
            rightItemViewModel.SetActionCommand(() => { Thread.Sleep(70); });
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);
            rowViewModel.RightItem.AcceptCommand.Execute(null);
            Thread.Sleep(25); //  Чтобы успели обновиться свойства.

            Assert.False(rowViewModel.CommandButtonIsVisible);
            Assert.True(rowViewModel.ProcessIconIsVisible);
        }

        /// <summary>
        /// Тест на простановку свойств видимости после завершения принятия левого элемента.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_FinishedLeftItemAccept()
        {
            var leftItemViewModel = new ItemViewModel("LeftItem", false, null);
            var rightItemViewModel = new TestItemViewModel("RightItem", ItemStatusEnum.Older);
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);
            rowViewModel.LeftItem.AcceptCommand.Execute(null);
            Thread.Sleep(25); //  Чтобы успели обновиться свойства.

            Assert.False(rowViewModel.CommandButtonIsVisible);
            Assert.False(rowViewModel.ProcessIconIsVisible);
        }

        /// <summary>
        /// Тест на простановку свойств видимости после завершения принятия правого элемента.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_FinishedRightItemAccept()
        {
            var leftItemViewModel = new TestItemViewModel("LeftItem", ItemStatusEnum.Older);
            var rightItemViewModel = new ItemViewModel("RightItem", false, null);
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);
            rowViewModel.RightItem.AcceptCommand.Execute(null);
            Thread.Sleep(25); // Чтобы успели обновиться свойства.

            Assert.False(rowViewModel.CommandButtonIsVisible);
            Assert.False(rowViewModel.ProcessIconIsVisible);
        }

        private class TestItemViewModel : IItemViewModel
        {
            public TestItemViewModel(string name, ItemStatusEnum status)
            {
                Name = name;
                Status = new ItemStatus(status);
            }

            public string Name { get; }

            public IDirectory Directory => null;

            public ItemStatus Status { get; private set; }

            public Action CommandAction { get; }

            public ICommand AcceptCommand { get; set; }

            public string IconPath => throw new NotImplementedException();

            public bool IsDirectory { get; }

            public IItem Item => throw new NotImplementedException();

            public string FullPath => throw new NotImplementedException();

            public event PropertyChangedEventHandler PropertyChanged;
            public event Action StartedSyncEvent;
            public event Action FinishedSyncEvent;
            public event Action ItemIsDeletedEvent;
            public event Action<IItemViewModel, IItemViewModel> CopiedFromToEvent;
            public event Action AcceptCommandChangedEvent;

            public void SetActionCommand(Action action) { }

            public void UpdateStatus(ItemStatusEnum statusEnum)
            {
                Status = new ItemStatus(statusEnum);
            }
        }

        private class TestItem : IItem
        {
            public string Name { get; private set; }

            public string FullPath => throw new NotImplementedException();

            public DateTime LastUpdate => throw new NotImplementedException();

            public event Action DeletedEvent;
            public event Action<string> SyncErrorEvent;
            public event Action<IItem, IItem, string> CopiedFromToEvent;

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