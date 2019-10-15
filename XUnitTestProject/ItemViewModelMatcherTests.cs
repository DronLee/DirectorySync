using DirectorySync.Models;
using DirectorySync.ViewModels;
using System;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject
{
    public class ItemViewModelMatcherTests
    {
        [Theory]
        [InlineData("2019-01-01", "2018-01-01", "Newer", "Older")]
        [InlineData("2018-01-01", "2019-01-01", "Older", "Newer")]
        [InlineData("2019-01-01", null, "ThereIs", "Missing")]
        [InlineData(null, "2019-01-01", "Missing", "ThereIs")]
        public void UpdateStatusesAndCommands(string date1, string date2, string strExpextedStatus1, string strExpextedStatus2)
        {
            var expextedStatus1 = Enum.Parse<ItemStatusEnum>(strExpextedStatus1);
            var expextedStatus2 = Enum.Parse<ItemStatusEnum>(strExpextedStatus2);

            var item1 = new ItemViewModel(null, false, date1 == null ? null :
                 new TestItem(DateTime.Parse(date1)));
            var item2 = new ItemViewModel(null, false, date2 == null ? null :
                 new TestItem(DateTime.Parse(date2)));

            var matcher = new ItemViewModelMatcher();
            matcher.UpdateStatusesAndCommands(item1, item2);

            Assert.Equal(expextedStatus1, item1.Status.StatusEnum);
            Assert.Equal(expextedStatus2, item2.Status.StatusEnum);
            Assert.NotNull(item1.AcceptCommand);
            Assert.NotNull(item2.AcceptCommand);
        }

        [Fact]
        public void UpdateStatusesAndCommandsForEquals()
        {
            var lastUpdate = DateTime.Now;

            var item1 = new ItemViewModel(null, false, new TestItem(lastUpdate));
            var item2 = new ItemViewModel(null, false, new TestItem(lastUpdate));
            item1.SetActionCommand(() => { });
            item2.SetActionCommand(() => { });

            var matcher = new ItemViewModelMatcher();
            matcher.UpdateStatusesAndCommands(item1, item2);

            Assert.Equal(ItemStatusEnum.Equally, item1.Status.StatusEnum);
            Assert.Equal(ItemStatusEnum.Equally, item2.Status.StatusEnum);
            Assert.Null(item1.AcceptCommand);
            Assert.Null(item2.AcceptCommand);
        }

        private class TestItem : IItem
        {
            public TestItem(DateTime lastUpdate)
            {
                LastUpdate = lastUpdate;
            }

            public string Name => throw new NotImplementedException();

            public string FullPath => throw new NotImplementedException();

            public DateTime LastUpdate { get; }

            public event Action DeletedEvent;
            public event Action<IItem, IItem, string> CopiedFromToEvent;
            public event Action<string> SyncErrorEvent;

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