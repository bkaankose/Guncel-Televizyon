using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GTVWin8.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        #region Properties
        private bool _isNetworkAvailable;

        public bool IsNetworkAvailable
        {
            get { return _isNetworkAvailable; }
            set { _isNetworkAvailable = value; NotifyPropertyChanged("IsNetworkAvailable"); }
        }

        #endregion
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
