namespace DirectorySync.Models.Settings
{
    public class SettingsRow : ISettingsRow
    {
        public SettingsDirectory LeftDirectory { get; set; }
        public SettingsDirectory RightDirectory { get; set; }
        public bool IsUsed { get; set; }
    }
}