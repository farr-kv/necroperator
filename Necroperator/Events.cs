using Microsoft.Extensions.Logging;

namespace Necroperator
{
    internal class Events
    {
        internal record Log(LogLevel Level, string Message);
        internal record BackupCreated();

        public static Log Info(string message) => new(LogLevel.Information, message);
        public static Log Warn(string message) => new(LogLevel.Warning, message);
        public static Log Error(string message) => new(LogLevel.Error, message);
    }
}
