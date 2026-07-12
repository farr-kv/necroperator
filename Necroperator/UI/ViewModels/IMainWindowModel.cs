namespace Necroperator.UI.ViewModels
{
    public interface IMainWindowModel
    {
        bool AutoScrollLogs { get; set; }
        bool IsRunning { get; }
        string SaveLocation { get; set; }
        DateTimeOffset? LastBackupDate { get; set; }

        void StartWatching();
        void StopWatching();
    }
}