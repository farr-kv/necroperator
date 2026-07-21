using Microsoft.Win32;
using Necroperator.Extensions;
using Necroperator.Models;
using Necroperator.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Necroperator.UI.Windows.Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly string[] GameIds = [
            "1771", // Ubiconnect
            "3559"  // Steam
        ];
        private readonly IUbisoftService ubisoftService;
        private readonly IBackupManager backupManager;
        private readonly IEnumerable<IDisposable> disposables;
        
        private readonly IFileMonitor fileMonitor;

        public string SaveLocation
        {
            get => this.backupManager.SaveDirectory;
            set
            {
                this.backupManager.SaveDirectory = value;
                this.RaisePropertyChanged();
                this.InvalidateSnapshots();
            }
        }

        public bool IsRunning
        {
            get => fileMonitor.IsRunning;
        }

        public ObservableCollection<Snapshot> Snapshots { get; } = [];

        public MainWindow(IUbisoftService ubisoftService, IEventBus eventBus, IFileMonitor fileMonitor, IBackupManager backupManager)
        {
            this.ubisoftService = ubisoftService;
            this.fileMonitor = fileMonitor;
            this.backupManager = backupManager;
            this.disposables = [
                eventBus.RegisterForEvent<UIEvents.BackupCreated>(OnBackupCreated)
            ];

            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ubisoftService.TryGetInstallationPath(out var ubiPath))
            {
                ubiPath = Path.Combine(ubiPath, "savegames");
                foreach (var id in GameIds)
                {
                    var path = Directory.GetDirectories(ubiPath, id, SearchOption.AllDirectories).FirstOrDefault();
                    if (path is not null)
                    {
                        this.SaveLocation = path;
                        break;
                    }
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            foreach (var disposable in disposables)
                disposable.Dispose();
            base.OnClosed(e);
        }

        private void Titlebar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                InitialDirectory = this.SaveLocation,
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                this.SaveLocation = dialog.FolderName;
            }   
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            this.fileMonitor.Start(this.SaveLocation);

            if (IsRunning)
                this.RaisePropertyChanged(nameof(IsRunning));
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            this.fileMonitor.Stop();

            if (!IsRunning)
                this.RaisePropertyChanged(nameof(IsRunning));
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.InvalidateSnapshots();
        }

        private void ButtonManualSave_Click(object sender, RoutedEventArgs e)
        {
            this.backupManager.CreateBackup();
        }

        private void ButtonSnapshotRestore_Click(object sender, RoutedEventArgs e)
        {
            var snapshot = ((Control)sender).DataContext as Snapshot;
            this.backupManager.RestoreBackup(snapshot!);
        }

        private void ButtonSnapshotDelete_Click(object sender, RoutedEventArgs e)
        {
            var snapshot = ((Control)sender).DataContext as Snapshot;
            this.backupManager.DeleteBackup(snapshot!);
            this.InvalidateSnapshots();
        }

        private void ButtonSnapshotOpen_Click(object sender, RoutedEventArgs e)
        {
            var snapshot = ((Control)sender).DataContext as Snapshot;
            Process.Start("explorer.exe", snapshot!.Path);
        }

        private void InvalidateSnapshots()
        {
            this.Snapshots.Clear();
            foreach (var snapshot in this.backupManager.ListBackups())
            {
                this.Snapshots.Add(snapshot);
            }
        }

        private void OnBackupCreated(DateTimeOffset timestamp, UIEvents.BackupCreated msg)
        {
            // Clear old snapshots that might've been deleted
            var toRemove = this.Snapshots.Where(x => x.IsExpired).ToList();

            this.RunOnUiThread(() =>
            {
                foreach (var snapshot in toRemove)
                {
                    this.Snapshots.Remove(snapshot);
                }
                this.Snapshots.Insert(0, msg.Snapshot);
            });
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}