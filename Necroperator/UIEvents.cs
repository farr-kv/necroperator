using Microsoft.Extensions.Logging;
using Necroperator.Models;

namespace Necroperator
{
    internal class UIEvents
    {
        internal record BackupCreated(Snapshot Snapshot);
        internal record BackupRestored();
    }
}
