using DirectorySync.Models.Settings;
using System.Threading.Tasks;

namespace DirectorySync.Models
{
    /// <summary>
    /// Класс описывает пару синхронизируемых директорий.
    /// </summary>
    public class SynchronizedDirectories : ISynchronizedDirectories
    {
        public SynchronizedDirectories(ISettingsRow settingsRow, IItemFactory itemFactory)
        {
            LeftDirectory = itemFactory.CreateDirectory(settingsRow.LeftDirectory.DirectoryPath, settingsRow.ExcludedExtensions);
            RightDirectory = itemFactory.CreateDirectory(settingsRow.RightDirectory.DirectoryPath, settingsRow.ExcludedExtensions);
        }

        /// <summary>
        /// Левая директория.
        /// </summary>
        public IDirectory LeftDirectory { get; }

        /// <summary>
        /// Правая директория.
        /// </summary>
        public IDirectory RightDirectory { get; }

        /// <summary>
        /// Директории загружены.
        /// </summary>
        public bool IsLoaded { get; private set; } = false;

        /// <summary>
        /// Загрузка директорий.
        /// </summary>
        public async Task Load()
        {
            await Task.Run(() => Task.WaitAll(LeftDirectory.Load(), RightDirectory.Load()));
            IsLoaded = true;
        }

        /// <summary>
        /// Пометка о том, что требуется загрузка для пары синхронизируемых директорий.
        /// </summary>
        public void LoadRequired()
        {
            IsLoaded = false;
        }
    }
}