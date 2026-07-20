using Microsoft.Extensions.Logging;
using Necroperator.Models;

namespace Necroperator
{
    internal class UIEvents
    {
        internal record Log(LogLevel Level, string Message);
        internal record BackupCreated(Snapshot Snapshot);
        internal record BackupRestored();

        public static Log Info(string message) => new(LogLevel.Information, message);
        public static Log Warn(string message) => new(LogLevel.Warning, message);
        public static Log Error(string message) => new(LogLevel.Error, message);
    }
}
