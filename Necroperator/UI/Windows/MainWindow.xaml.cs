using Microsoft.Win32;
using Necroperator.Extensions;
using Necroperator.Services;
using Necroperator.UI.Controls;
using Necroperator.UI.ViewModels;
using System.Windows;

namespace Necroperator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IMainWindowModel Model => (IMainWindowModel)this.DataContext;
        private readonly IEnumerable<IDisposable> disposables;

        public MainWindow(IEventBus eventBus, IMainWindowModel model)
        {
            this.DataContext = model;
            InitializeComponent();

            this.disposables = [
                eventBus.RegisterForEvent<Events.Log>(OnLog),
                eventBus.RegisterForEvent<Events.BackupCreated>(OnBackupCreated)
            ];
        }

        protected override void OnClosed(EventArgs e)
        {
            foreach (var disposable in disposables)
                disposable.Dispose();
            base.OnClosed(e);
        }

        private void GridTitlebar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
                InitialDirectory = this.Model.SaveLocation,
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                this.Model.SaveLocation = dialog.FolderName;
            }   
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            this.Model.StartWatching();
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            this.Model.StopWatching();
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            this.Container_Logs.Children.Clear();
        }

        private void OnLog(DateTimeOffset timestamp, Events.Log msg)
        {
            this.RunOnUiThread(() => {
                var element = new LogEventControl(timestamp, msg.Level, msg.Message);
                this.Container_Logs.Children.Add(element);

                if (this.Model.AutoScrollLogs)
                {
                    element.BringIntoView();
                }
            });
        }

        private void OnBackupCreated(DateTimeOffset timestamp, Events.BackupCreated msg)
        {
            this.RunOnUiThread(() => this.Model.LastBackupDate = timestamp);
        }
    }
}