using DirectorySync.Models;
using DirectorySync.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject
{
    public class RowViewModelTests
    {
        ///// <summary>
        ///// Проверка простановки статусов и отсутствия команд при выполнении RefreshStatusesFromChilds для строки,
        ///// содержащей строку с неопределённым статусом.
        ///// </summary>
        //[Fact]
        //public void RefreshStatusesFromChilds_UnknownChild()
        //{
        //    // Начальный статус элементов не важен в данном случае, всё равно должен будет замениться.
        //    var parentRowViewModel = new RowViewModel(new TestItemViewModel("Left", ItemStatusEnum.Missing),
        //        new TestItemViewModel("Right", ItemStatusEnum.Missing), null);

        //    var childRow = new RowViewModel(
        //        new TestItemViewModel("Left", ItemStatusEnum.Unknown),
        //        new TestItemViewModel("Right", ItemStatusEnum.Unknown), parentRowViewModel);
        //    parentRowViewModel.ChildRows.Add(childRow);

        //    #region Наполнение дочерней строки своими строками с командами
        //    var leftItem = new TestItemViewModel("Left1", ItemStatusEnum.Newer);
        //    leftItem.SetActionCommand(() => { return Task.FromResult(true); });
        //    var rightItem = new TestItemViewModel("Right1", ItemStatusEnum.Older);
        //    rightItem.SetActionCommand(() => { return Task.FromResult(true); });
        //    childRow.ChildRows.Add(new RowViewModel(leftItem, rightItem, childRow));

        //    leftItem = new TestItemViewModel("Left2", ItemStatusEnum.Missing);
        //    leftItem.SetActionCommand(() => { return Task.FromResult(true); });
        //    rightItem = new TestItemViewModel("Right2", ItemStatusEnum.ThereIs);
        //    rightItem.SetActionCommand(() => { return Task.FromResult(true); });
        //    childRow.ChildRows.Add(new RowViewModel(leftItem, rightItem, childRow));
        //    #endregion

        //    parentRowViewModel.RefreshStatusesFromChilds();

        //    // У дочерней строки должен остаться неопределённый статус.
        //    Assert.Equal(ItemStatusEnum.Unknown, childRow.LeftItem.Status.StatusEnum);
        //    Assert.Equal(ItemStatusEnum.Unknown, childRow.RightItem.Status.StatusEnum);

        //    // Статусы родительской строки должны измениться на неопредёлённые.
        //    Assert.Equal(ItemStatusEnum.Unknown, parentRowViewModel.LeftItem.Status.StatusEnum);
        //    Assert.Equal(ItemStatusEnum.Unknown, parentRowViewModel.RightItem.Status.StatusEnum);

        //    // Команд не должно быть ни у дочерней строки, ни у родительской.
        //    Assert.Null(childRow.LeftItem.CommandAction);
        //    Assert.Null(childRow.RightItem.CommandAction);
        //    Assert.Null(parentRowViewModel.LeftItem.CommandAction);
        //    Assert.Null(parentRowViewModel.RightItem.CommandAction);
        //}

        /// <summary>
        /// Тест на выполнение команды принятия левого элемента.
        /// </summary>
        [Fact]
        public void LeftItemAcceptCommand()
        {
            var useAcceptCommand = false;

            var leftSynchronizedItem = new SynchronizedItem("LeftItem", false, null);
            var rightSynchronizedItem = new SynchronizedItem("RightItem", false, null);
            var leftItemViewModel = new ItemViewModel(leftSynchronizedItem);
            var rightItemViewModel = new ItemViewModel(rightSynchronizedItem);

            leftSynchronizedItem.SyncCommand.SetCommandAction(() => Task.Run(() => { useAcceptCommand = true; }));
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);
            rowViewModel.LeftItem.AcceptCommand.Execute(null);
            Thread.Sleep(15); //  Чтобы успела выполниться команда.

            Assert.True(useAcceptCommand);
        }

        /// <summary>
        /// Тест на выполнение команды принятия правого элемента.
        /// </summary>
        [Fact]
        public void RightItemAcceptCommand()
        {
            var useAcceptCommand = false;

            var leftSynchronizedItem = new SynchronizedItem("LeftItem", false, null);
            var rightSynchronizedItem = new SynchronizedItem("RightItem", false, null);
            var leftItemViewModel = new ItemViewModel(leftSynchronizedItem);
            var rightItemViewModel = new ItemViewModel(rightSynchronizedItem);

            rightSynchronizedItem.SyncCommand.SetCommandAction(() => Task.Run(() => { useAcceptCommand = true; }));
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);
            rowViewModel.RightItem.AcceptCommand.Execute(null);
            Thread.Sleep(15); //  Чтобы успела выполниться команда.

            Assert.True(useAcceptCommand);
        }

        /// <summary>
        /// Тест на простановку свойств видимости при инициализации.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_Init()
        {
            var leftSynchronizedItem = new SynchronizedItem("LeftItem", false, null);
            var rightSynchronizedItem = new SynchronizedItem("RightItem", false, null);
            var leftItemViewModel = new ItemViewModel(leftSynchronizedItem);
            var rightItemViewModel = new ItemViewModel(rightSynchronizedItem);

            leftSynchronizedItem.SyncCommand.SetCommandAction(() => Task.Run(() => { }));

            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);

            // Когда строка только создана, она считается ещё в процессе загрузки. Этот признак должен быть потом убран.
            Assert.True(rowViewModel.InProcess);
            // Соответсвенно, пока строка "в процессе", кнопки синхронизации не должны быть доступны.
            Assert.False(rowViewModel.CommandButtonIsVisible);
        }

        /// <summary>
        /// Тест на простановку свойств видимости сразу после запуска принятия левого элемента.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_StartedLeftItemAccept()
        {
            var leftSynchronizedItem = new SynchronizedItem("LeftItem", false, null);
            var rightSynchronizedItem = new SynchronizedItem("RightItem", false, null);
            rightSynchronizedItem.UpdateStatus(ItemStatusEnum.Older);
            var leftItemViewModel = new ItemViewModel(leftSynchronizedItem);
            var rightItemViewModel = new ItemViewModel(rightSynchronizedItem);


            // Sleep, чтобы была возможность проверить свойства в процессе выполнения синхронизации.
            leftSynchronizedItem.SyncCommand.SetCommandAction(() => { Thread.Sleep(70); return Task.FromResult(true); });
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);
            rowViewModel.LeftItem.AcceptCommand.Execute(null);
            Thread.Sleep(25); //  Чтобы успели обновиться свойства.

            Assert.False(rowViewModel.CommandButtonIsVisible);
            Assert.True(rowViewModel.InProcess);
        }

        /// <summary>
        /// Тест на простановку свойств видимости сразу после запуска принятия правого элемента.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_StartedRightItemAccept()
        {
            var leftSynchronizedItem = new SynchronizedItem("LeftItem", false, null);
            leftSynchronizedItem.UpdateStatus(ItemStatusEnum.Older);
            var rightSynchronizedItem = new SynchronizedItem("RightItem", false, null);
            var leftItemViewModel = new ItemViewModel(leftSynchronizedItem);
            var rightItemViewModel = new ItemViewModel(rightSynchronizedItem);

            // Sleep, чтобы была возможность проверить свойства в процессе выполнения синхронизации.
            rightSynchronizedItem.SyncCommand.SetCommandAction(() => { Thread.Sleep(70); return Task.FromResult(true); });
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, null);
            rowViewModel.RightItem.AcceptCommand.Execute(null);
            Thread.Sleep(25); //  Чтобы успели обновиться свойства.

            Assert.False(rowViewModel.CommandButtonIsVisible);
            Assert.True(rowViewModel.InProcess);
        }

        /// <summary>
        /// Тест на простановку свойств видимости после завершения принятия левого элемента.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_FinishedLeftItemAccept()
        {
            var parentRow = new RowViewModel(new ItemViewModel(new SynchronizedItem("LeftItem", false, new TestItem())),
                new ItemViewModel(new SynchronizedItem("RightItem", false, new TestItem())), null);

            var leftSynchronizedItem = new SynchronizedItem("LeftItem", false, new TestItem());
            var rightSynchronizedItem = new SynchronizedItem("RightItem", false, new TestItem());
            var leftItemViewModel = new ItemViewModel(leftSynchronizedItem);
            var rightItemViewModel = new ItemViewModel(rightSynchronizedItem);
            leftSynchronizedItem.SyncCommand.SetCommandAction(() => { return Task.FromResult(true); });
            rightSynchronizedItem.SyncCommand.SetCommandAction(() => { return Task.FromResult(true); });
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, parentRow);

            rowViewModel.LeftItem.AcceptCommand.Execute(null);
            rowViewModel.LoadFinished(); // Без этого InProcess не изменится.
            Thread.Sleep(10); //  Чтобы успели обновиться свойства.

            Assert.True(rowViewModel.CommandButtonIsVisible);
            Assert.False(rowViewModel.InProcess);
        }

        /// <summary>
        /// Тест на простановку свойств видимости после завершения принятия правого элемента.
        /// </summary>
        [Fact]
        public void VisibilityAcceptButton_FinishedRightItemAccept()
        {
            var parentRow = new RowViewModel(new ItemViewModel(new SynchronizedItem("LeftItem", false, new TestItem())),
                new ItemViewModel(new SynchronizedItem("RightItem", false, new TestItem())), null);

            var leftSynchronizedItem = new SynchronizedItem("LeftItem", false, new TestItem());
            var rightSynchronizedItem = new SynchronizedItem("RightItem", false, new TestItem());
            var leftItemViewModel = new ItemViewModel(leftSynchronizedItem);
            var rightItemViewModel = new ItemViewModel(rightSynchronizedItem);
            leftSynchronizedItem.SyncCommand.SetCommandAction(() => { return Task.FromResult(true); });
            rightSynchronizedItem.SyncCommand.SetCommandAction(() => { return Task.FromResult(true); });
            var rowViewModel = new RowViewModel(leftItemViewModel, rightItemViewModel, parentRow);

            rowViewModel.RightItem.AcceptCommand.Execute(null);
            rowViewModel.LoadFinished(); // Без этого InProcess не изменится.
            Thread.Sleep(10); //  Чтобы успели обновиться свойства.

            Assert.True(rowViewModel.CommandButtonIsVisible);
            Assert.False(rowViewModel.InProcess);
        }

        private class TestItem : IItem
        {
            public string Name { get; private set; }

            public string FullPath => throw new NotImplementedException();

            public DateTime LastUpdate => throw new NotImplementedException();

            public event Action<IItem> DeletedEvent;
            public event Action<string> SyncErrorEvent;
            public event Action<IItem, string> CopiedFromToEvent;

            public Task CopyTo(string destinationPath)
            {
                return Task.Run(() => CopiedFromToEvent?.Invoke(null, destinationPath));
            }

            public Task Delete()
            {
                throw new NotImplementedException();
            }
        }
    }
}