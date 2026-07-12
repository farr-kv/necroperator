using System.IO;

namespace Necroperator.Services.Implementations
{
    internal class FileMonitor : IFileMonitor, IDisposable
    {
        public bool IsRunning => this.fileWatcher.EnableRaisingEvents;
        
        private readonly IEventBus eventBus;
        private readonly FileSystemWatcher fileWatcher;

        public FileMonitor(IEventBus eventBus)
        {
            this.eventBus = eventBus;
            this.fileWatcher = new();
        }

        public void Start(string directory)
        {
            if (this.IsRunning)
                throw new Exception("Service is already started");

            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                this.eventBus.Publish(Events.Error("Invalid save location."));
                return;
            }

            this.fileWatcher.Path = directory;
            this.fileWatcher.IncludeSubdirectories = false;
            this.fileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            this.fileWatcher.EnableRaisingEvents = true;
            this.fileWatcher.Changed += OnFileChanged;
            this.fileWatcher.Created += OnFileCreated;

            this.eventBus.Publish(Events.Info("Started monitoring files"));
        }

        public void Stop()
        {
            if (!this.IsRunning)
                throw new Exception("Service is already stopped");

            this.fileWatcher.EnableRaisingEvents = false;
            this.eventBus.Publish(Events.Info("Stopped monitoring files"));
        }

        public void Dispose()
        {
            this.Stop();
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath.EndsWith(".delete"))
            {
                File.Delete(e.FullPath);
                var oldPath = e.FullPath.Remove(e.FullPath.Length - 7, 7); // remove .delete
                var backupPath = oldPath + ".backup";

                if (File.Exists(backupPath))
                {
                    try
                    {
                        File.Copy(backupPath, oldPath, true); // Restore backup
                        this.eventBus.Publish(Events.Info($"Restored backup file {Path.GetFileName(oldPath)}"));
                    }
                    catch
                    {
                        this.eventBus.Publish(Events.Warn($"Unable to restore backup file: {Path.GetFileName(backupPath)}"));
                    }
                }   
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath.EndsWith(".tmp"))
            {
                var oldPath = e.FullPath.Remove(e.FullPath.Length - 4, 4); // remove .tmp
                try
                {
                    File.Copy(oldPath, oldPath + ".backup", true); // Keep running backup of save file before update
                    this.eventBus.Publish(new Events.BackupCreated());
                }
                catch
                {
                    // During gameplay GRWL does not release the lock on the save file. Upon play death, this lock is released while creating the tmp/old/delete files
                }
            }
        }
    }
}
