using DirectorySync.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace XUnitTestProject
{
    /// <summary>
    /// Тесты команды синхронизации.
    /// </summary>
    public class SyncCommandTests
    {
        private readonly static List<string> _processOutputLines = new List<string>();

        private readonly static Func<Task>[] _actions =
        {
            null,
            () => { return Task.Run(() => _processOutputLines.Add("Process action1")); },
            () => { return Task.Run(() => _processOutputLines.Add("Process action2")); }
        };

        [Fact]
        public void Init()
        {
            var syncCommand = new SyncCommand();

            Assert.Null(syncCommand.CommandAction);
        }

        [Theory]
        [InlineData(0, 0, false)]
        [InlineData(1, 1, false)]
        [InlineData(0, 1, true)]
        [InlineData(1, 0, true)]
        [InlineData(1, 2, true)]
        public void SetCommandAction(byte oldActionIndex, byte newActionIndex, bool expectedActionChanged)
        {
            var useCommandActionChangedEvent = false;
            var syncCommand = new SyncCommand();
            syncCommand.SetCommandAction(_actions[oldActionIndex]);
            syncCommand.CommandActionChangedEvent += () => { useCommandActionChangedEvent = true; };
            syncCommand.SetCommandAction(_actions[newActionIndex]);

            Assert.Equal(_actions[newActionIndex], syncCommand.CommandAction);
            Assert.Equal(expectedActionChanged, useCommandActionChangedEvent);
        }

        [Fact]
        public async Task Process()
        {
            var syncCommand = new SyncCommand();
            syncCommand.SetCommandAction(_actions[1]);
            syncCommand.FinishedSyncEvent += () => { _processOutputLines.Add("Finished"); };
            await syncCommand.Process();

            Assert.Equal("Process action1|Finished", string.Join('|', _processOutputLines.ToArray()));
        }
    }
}