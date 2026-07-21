using System.IO;

namespace Necroperator.Models
{
    public record Snapshot(DateTimeOffset Timestamp, string Path, int NumberOfFiles, string FileHash)
    {
        public bool IsExpired => !Directory.Exists(Path);
    }
}
