namespace Necroperator.Services
{
    public interface IFileMonitor
    {
        bool IsRunning { get; }

        void Start(string directory);
        void Stop();
    }
}