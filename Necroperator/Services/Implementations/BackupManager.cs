using Microsoft.Extensions.Logging;
using Necroperator.Models;
using System.IO;
using System.Security.Cryptography;

namespace Necroperator.Services.Implementations
{
    internal class BackupManager : IBackupManager, IDisposable
    {
        public string SaveDirectory { get; set; } = string.Empty;
        public string BackupDirectory => Path.Combine(this.SaveDirectory, "Backups");
        public int MaxBackups { get; set; } = 50;

        private readonly IEventBus eventBus;
        private readonly ILogger<BackupManager> logger;
        private readonly HashAlgorithm hashAlgorithm = SHA256.Create();

        public BackupManager(IEventBus eventBus, ILogger<BackupManager> logger)
        {
            this.eventBus = eventBus;
            this.logger = logger;
        }

        public void Dispose()
        {
            this.hashAlgorithm.Dispose();
        }

        public void CreateBackup()
        {
            using (this.logger.BeginScope("CreateBackup"))
            {
                this.logger.LogInformation("Create snapshot started.");
                string? backupPath = null;

                try
                {
                    // Create new backup folder
                    var hashValue = this.ComputeFileHash(this.SaveDirectory);
                    this.logger.LogInformation("Computed hash: {0}", hashValue);
                    backupPath = Path.Combine(this.BackupDirectory, hashValue);
                    if (Directory.Exists(backupPath))
                    {
                        this.logger.LogInformation("Create snapshot aborted: Backup already exists");
                        return;
                    }
                    Directory.CreateDirectory(backupPath);

                    // Copy all .save files to the new backup folder
                    var files = Directory.GetFiles(this.SaveDirectory, "*.save", SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        var destination = Path.Combine(backupPath, Path.GetFileName(file));
                        this.logger.LogDebug("Copying '{0, -200}' => '{1, -200}'", file, destination);
                        File.Copy(file, destination);
                    }
                    this.logger.LogInformation("Copied {0} files to backup", files.Length);

                    // Cleanup old backups
                    var info = new DirectoryInfo(this.BackupDirectory);
                    var backupsToDelete = info.GetDirectories()
                        .OrderByDescending(p => p.CreationTime)
                        .Skip(this.MaxBackups)
                        .ToList();
                    foreach (var dir in backupsToDelete)
                    {
                        this.logger.LogDebug("Deleting '{0, -200}'", dir.FullName);
                        Directory.Delete(dir.FullName, true);
                    }
                    this.logger.LogInformation("Deleted {0} old backups", backupsToDelete.Count);

                    var snapshot = this.CreateSnapshotFromPath(backupPath);
                    this.eventBus.Publish(new UIEvents.BackupCreated(snapshot));
                }
                catch (Exception e)
                {
                    this.logger.LogError("Unexpected exception: {0}", e);

                    if (!string.IsNullOrEmpty(backupPath))
                    {
                        this.logger.LogInformation("Cleaning failed backup directory");
                        Directory.Delete(backupPath, true);
                    }
                }

                this.logger.LogInformation("Create snapshot completed.");
            }
        }

        public void RestoreBackup(Snapshot snapshot)
        {
            using (this.logger.BeginScope("RestoreBackup: {0}", snapshot.FileHash))
            {
                try
                {
                    this.logger.LogInformation("Restoring snapshot started.");

                    if (!Directory.Exists(snapshot.Path))
                    {
                        this.logger.LogWarning("Backup folder '{0}' does not exist.", snapshot.Path);
                        return;
                    }

                    // Clear the save directory before restoring
                    var toDelete = Directory.GetFiles(this.SaveDirectory);
                    foreach (var file in toDelete)
                    {
                        this.logger.LogDebug("Deleting '{0, -200}'", file);
                        File.Delete(file);
                    }
                    this.logger.LogInformation("Deleted {0} files", toDelete.Length);

                    // Copy all files from the backup folder to the save directory
                    var files = Directory.GetFiles(snapshot.Path);
                    foreach (var file in files)
                    {
                        var destination = Path.Combine(this.SaveDirectory, Path.GetFileName(file));
                        this.logger.LogDebug("Copying '{0, -200}' => '{1, -200}'", file, destination);
                        File.Copy(file, destination, true);
                    }
                    this.logger.LogInformation("Copied {0} backup files", files.Length);

                    this.logger.LogInformation("Restoring snapshot completed");
                    this.eventBus.Publish(new UIEvents.BackupRestored());
                }
                catch (Exception e)
                {
                    this.logger.LogError("Unexpected exception: {0}", e);
                }
            }
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
                            .Select(this.CreateSnapshotFromPath)
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

        private Snapshot CreateSnapshotFromPath(string path)
        {
            return new Snapshot(File.GetCreationTime(path), path, Directory.GetFiles(path).Length, Path.GetFileName(path));
        }
    }
}
