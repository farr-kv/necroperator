using System.IO;

namespace Necroperator.Services.Implementations
{
    internal class FileMonitor : IFileMonitor, IDisposable
    {
        public bool IsRunning => this.fileWatcher.EnableRaisingEvents;
        
        private readonly IEventBus eventBus;
        private readonly IBackupManager backupManager;
        private readonly FileSystemWatcher fileWatcher;

        public FileMonitor(IEventBus eventBus, IBackupManager backupManager)
        {
            this.eventBus = eventBus;
            this.backupManager = backupManager;
            this.fileWatcher = new();
        }

        public void Start(string directory)
        {
            if (this.IsRunning)
                throw new Exception("Service is already started");

            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                this.eventBus.Publish(UIEvents.Error("Invalid save location."));
                return;
            }

            this.fileWatcher.Path = directory;
            this.fileWatcher.IncludeSubdirectories = false;
            this.fileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            this.fileWatcher.EnableRaisingEvents = true;
            this.fileWatcher.Renamed += OnFileRenamed;

            this.eventBus.Publish(UIEvents.Info("Started monitoring files"));
        }

        public void Stop()
        {
            if (!this.IsRunning)
                throw new Exception("Service is already stopped");

            this.fileWatcher.EnableRaisingEvents = false;
            this.eventBus.Publish(UIEvents.Info("Stopped monitoring files"));
        }

        public void Dispose()
        {
            this.Stop();
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            switch (Path.GetExtension(e.FullPath))
            {
                case ".save":
                    this.backupManager.CreateBackup();
                    break;

                case ".delete":
                    // TODO Restore latest backup if available
                    break;
            }
        }
    }
}
