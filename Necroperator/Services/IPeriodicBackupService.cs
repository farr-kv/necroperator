namespace Necroperator.Services
{
    public interface IPeriodicBackupService
    {
        bool IsRunning { get; }

        void Start(string directory);
        void Stop();
    }
}