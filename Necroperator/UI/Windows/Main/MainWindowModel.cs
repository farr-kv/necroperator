using Necroperator.Services;
using System.IO;

namespace Necroperator.UI.Windows.Main
{
    public class MainWindowModel : BaseViewModel
    {
        private readonly string[] GameIds = [
            "1771", // ubiconnect
            "3559"  //Steam
        ];
        private readonly IFileMonitor fileMonitor;
        private readonly IPeriodicBackupService backupService;
        private bool autoScrollLogs = true;
        private string saveLocation = string.Empty;
        private DateTimeOffset? lastBackupDate = null;

        public string SaveLocation
        {
            get => saveLocation;
            set
            {
                saveLocation = value;
                this.RaisePropertyChanged();
            }
        }
        public bool IsRunning
        {
            get => fileMonitor.IsRunning || backupService.IsRunning;
        }

        public bool AutoScrollLogs
        {
            get => autoScrollLogs;
            set
            {
                autoScrollLogs = value;
                this.RaisePropertyChanged();
            }
        }

        public DateTimeOffset? LastBackupDate { 
            get => lastBackupDate;
            set 
            {
                lastBackupDate = value;
                this.RaisePropertyChanged();
            }
        }

        public MainWindowModel(IFileMonitor fileMonitor, IPeriodicBackupService backupService)
        {
            this.fileMonitor = fileMonitor;
            this.backupService = backupService;

            var ubiPath = @"C:\Program Files (x86)\Ubisoft\Ubisoft Game Launcher\savegames";
            foreach (var id in GameIds)
            {
                var path = Directory.GetDirectories(ubiPath, id, SearchOption.AllDirectories).FirstOrDefault();
                if (path is not null)
                {
                    this.SaveLocation = path;
                    return;
                }
            }

            this.backupService = backupService;
        }

        public void StartWatching()
        {
            this.fileMonitor.Start(this.SaveLocation);
            this.backupService.Start(this.SaveLocation);

            if (IsRunning)
                this.RaisePropertyChanged(nameof(IsRunning));
        }

        public void StopWatching()
        {
            this.fileMonitor.Stop();
            this.backupService.Stop();

            if (!IsRunning)
                this.RaisePropertyChanged(nameof(IsRunning));
        }
    }
}
