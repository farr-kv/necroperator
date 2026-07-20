using Necroperator.Models;

namespace Necroperator.Services
{
    public interface IBackupManager
    {
        string SaveDirectory { get; set; }

        void CreateBackup();
        void RestoreBackup(Snapshot snapshot);
        void DeleteBackup(Snapshot snapshot);
        IEnumerable<Snapshot> ListBackups();
    }
}