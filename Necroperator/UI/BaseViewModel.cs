using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Necroperator.UI
{
    public abstract class BaseViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
