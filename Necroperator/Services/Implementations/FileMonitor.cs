using Microsoft.Extensions.Logging;
using System.IO;

namespace Necroperator.Services.Implementations
{
    internal class FileMonitor : IFileMonitor, IDisposable
    {
        public bool IsRunning => this.fileWatcher.EnableRaisingEvents;
        
        private readonly ILogger<FileMonitor> logger;
        private readonly IBackupManager backupManager;
        private readonly FileSystemWatcher fileWatcher;

        public FileMonitor(IEventBus eventBus, IBackupManager backupManager, ILogger<FileMonitor> logger)
        {
            this.logger = logger;
            this.backupManager = backupManager;
            this.fileWatcher = new();
        }

        public void Start(string directory)
        {
            if (this.IsRunning)
                throw new Exception("Service is already started");

            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                this.logger.LogWarning("Invalid save location.");
                return;
            }

            this.fileWatcher.Path = directory;
            this.fileWatcher.IncludeSubdirectories = false;
            this.fileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            this.fileWatcher.EnableRaisingEvents = true;
            this.fileWatcher.Renamed += OnFileRenamed;

            this.logger.LogInformation("Started monitoring files");
        }

        public void Stop()
        {
            if (!this.IsRunning)
                throw new Exception("Service is already stopped");

            this.fileWatcher.EnableRaisingEvents = false;
            this.logger.LogInformation("Stopped monitoring files");
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
