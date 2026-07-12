using System.IO;

namespace Necroperator.Services.Implementations
{
    internal class PeriodicBackupService : IDisposable, IPeriodicBackupService
    {
        private const int BackupIntervalInMilliseconds = 600_000; // 10 minutes
        private const int MaxBackups = 20;

        public bool IsRunning { get; private set; }

        private readonly IEventBus eventBus;
        private readonly Timer timer;

        private string directory = string.Empty;

        public PeriodicBackupService(IEventBus eventBus)
        {
            this.eventBus = eventBus;
            this.timer = new Timer(_ =>
            {
                // Create new backup folder
                var backupLocation = Path.Combine(directory, "backups");
                var timestamp = DateTimeOffset.Now.ToString("yyyyMMddHHmmss");
                Directory.CreateDirectory(Path.Combine(backupLocation, timestamp));

                // Copy all .save files to the new backup folder
                var files = Directory.GetFiles(directory, "*.save", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    File.Copy(file, Path.Combine(backupLocation, timestamp, Path.GetFileName(file)));
                }
                this.eventBus.Publish(new Events.BackupCreated());

                // Cleanup old backups
                var backupsToDelete = Directory.GetDirectories(backupLocation)
                                            .OrderDescending()
                                            .Skip(MaxBackups);
                foreach (var path in backupsToDelete)
                {
                    Directory.Delete(path, true);
                }
            });
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

            this.directory = directory;
            this.IsRunning = timer.Change(0, BackupIntervalInMilliseconds);
        }

        public void Stop()
        {
            if (!this.IsRunning)
                throw new Exception("Service is already stopped");

            this.IsRunning = !timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
