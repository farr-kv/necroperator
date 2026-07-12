namespace Necroperator.Services
{
    internal interface IPeriodicBackupService
    {
        bool IsRunning { get; }

        void Start(string directory);
        void Stop();
    }
}