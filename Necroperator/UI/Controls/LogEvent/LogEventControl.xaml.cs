using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Necroperator.UI.Controls.LogEvent
{
    /// <summary>
    /// Interaction logic for LogEventControl.xaml
    /// </summary>
    public partial class LogEventControl : UserControl, INotifyPropertyChanged
    {
        public DateTimeOffset Timestamp { get; set; }
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; }

        public LogEventControl(DateTimeOffset timestamp, LogLevel logLevel, string message)
        {
            this.Timestamp = timestamp;
            this.LogLevel = logLevel;
            this.Message = message;

            InitializeComponent();
            RaisePropertyChanged(nameof(Message));
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {

            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged
    }
}
