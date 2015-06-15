using GTVWin8.DataModels;
using SM.Media.Playlists;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GTVWin8.ViewModels
{
    public class WatchPageViewModel : INotifyPropertyChanged
    {
        #region Props
        private ISubProgram _selectedBandwith;

        public ISubProgram SelectedBandwith
        {
            get { return _selectedBandwith; }
            set
            {
                _selectedBandwith = value;
                NotifyPropertyChanged();
            }
        }

        private double _volume = 100;
        public double Volume
        {
            get { return _volume; }
            set { _volume = value; NotifyPropertyChanged(); }
        }

        private ObservableCollection<ChannelStreamProgram> _allCurrenStreams;

        public ObservableCollection<ChannelStreamProgram> AllCurrentStreams
        {
            get { return _allCurrenStreams; }
            set { _allCurrenStreams = value; NotifyPropertyChanged(); }
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
        public WatchPageViewModel()
        {

        }
    }
}
