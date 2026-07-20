using Necroperator.Models;
using System.IO;
using System.Security.Cryptography;

namespace Necroperator.Services.Implementations
{
    internal class BackupManager : IBackupManager, IDisposable
    {
        public string SaveDirectory { get; set; } = string.Empty;
        public string BackupDirectory => Path.Combine(this.SaveDirectory, "Backups");
        public int MaxBackups { get; set; } = 20;

        private readonly IEventBus eventBus;
        private readonly HashAlgorithm hashAlgorithm = SHA256.Create();

        public BackupManager(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public void Dispose()
        {
            this.hashAlgorithm.Dispose();
        }

        public void CreateBackup()
        {
            // Create new backup folder
            var hashValue = this.ComputeFileHash(this.SaveDirectory);
            var backupPath = Path.Combine(this.BackupDirectory, hashValue);
            if (Directory.Exists(backupPath))
                return; // This snapshot has already been taken

            Directory.CreateDirectory(backupPath);

            // Copy all .save files to the new backup folder
            var files = Directory.GetFiles(this.SaveDirectory, "*.save", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                File.Copy(file, Path.Combine(backupPath, Path.GetFileName(file)));
            }

            // Cleanup old backups
            var info = new DirectoryInfo(this.BackupDirectory);
            var backupsToDelete = info.GetDirectories()
                .OrderByDescending(p => p.CreationTime)
                .Skip(this.MaxBackups);
            foreach (var dir in backupsToDelete)
            {
                Directory.Delete(dir.FullName, true);
            }

            var snapshot = new Snapshot(File.GetCreationTime(backupPath), backupPath, hashValue);
            this.eventBus.Publish(new UIEvents.BackupCreated(snapshot));
        }

        public void RestoreBackup(Snapshot snapshot)
        {
            if (!Directory.Exists(snapshot.Path))
            {
                this.eventBus.Publish(UIEvents.Error("Backup folder does not exist."));
                return;
            }

            // Clear the save directory before restoring
            var toDelete = Directory.GetFiles(this.SaveDirectory);
            foreach (var file in toDelete)
            {
                File.Delete(file);
            }

            // Copy all files from the backup folder to the save directory
            var files = Directory.GetFiles(snapshot.Path);
            foreach (var file in files)
            {
                File.Copy(file, Path.Combine(this.SaveDirectory, Path.GetFileName(file)), true);
            }
            this.eventBus.Publish(new UIEvents.BackupRestored());
        }

        public void DeleteBackup(Snapshot snapshot)
        {
            Directory.Delete(snapshot.Path, true);
        }

        public IEnumerable<Snapshot> ListBackups()
        {
            if (!Directory.Exists(this.BackupDirectory))
                return Enumerable.Empty<Snapshot>();

            return Directory.GetDirectories(this.BackupDirectory)
                            .Where(x => !string.IsNullOrEmpty(x))
                            .Select(x => new Snapshot(File.GetCreationTime(x), x, Path.GetFileName(x)))
                            .OrderByDescending(x => x.Timestamp);
        }

        private string ComputeFileHash(string directoryPath)
        {
            using var stream = new MemoryStream();
            foreach (var file in Directory.GetFiles(directoryPath, "*.save", SearchOption.TopDirectoryOnly).Order())
            {
                stream.Write(File.ReadAllBytes(file));
            }           

            var data = hashAlgorithm.ComputeHash(stream.ToArray());
            return BitConverter.ToString(data).Replace("-", "");
        }
    }
}
