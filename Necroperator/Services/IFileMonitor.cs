namespace Necroperator.Services
{
    internal interface IFileMonitor
    {
        bool IsRunning { get; }

        void Start(string directory);
        void Stop();
    }
}