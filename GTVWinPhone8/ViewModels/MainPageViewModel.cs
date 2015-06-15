using GTVWinPhone8.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GTVWinPhone8.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {

        private ObservableCollection<Channels> _allChannels;

        public ObservableCollection<Channels> AllChannels
        {
            get { return _allChannels; }
            set { _allChannels = value; NotifyPropertyChanged(); }
        }
        

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
