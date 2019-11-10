using System.Threading.Tasks;

namespace DirectorySync.Models
{
    /// <summary>
    /// Интерфейс описывает пару синхронизируемых директорий.
    /// </summary>
    public interface ISynchronizedDirectories
    {
        /// <summary>
        /// Директории загружены.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Левая директория.
        /// </summary>
        IDirectory LeftDirectory { get; }

        /// <summary>
        /// Правая директория.
        /// </summary>
        IDirectory RightDirectory { get; }

        /// <summary>
        /// Загрузка директорий.
        /// </summary>
        Task Load();

        /// <summary>
        /// Пометка о том, что требуется загрузка для пары синхронизируемых директорий.
        /// </summary>
        void LoadRequired();
    }
}