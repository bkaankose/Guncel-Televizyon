using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace GTVWin8.DataModels
{
    public enum Category
    {
        Ulusal,
        Haber,
        Sinema,
        Spor,
        Belgesel,
        Çocuk,
        Müzik,
        Eğlence,
        Moda
    }
    public class Channels : INotifyPropertyChanged
    {
        public Channels()
        {
            AllCurrentPrograms = new ObservableCollection<ChannelStreamProgram>();
        }
        private int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; OnPropertyChanged(); }
        }

        private bool _isActive;

        public bool IsActive
        {
            get { return _isActive; }
            set { 
                _isActive = value;
                OnPropertyChanged();
            }
        }
        
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(); }
        }

        private string _Url;

        public string Url
        {
            get { return _Url; }
            set { _Url = value; OnPropertyChanged(); }
        }

        private Category _Category;
        public Category Category
        {
            get { return _Category; }
            set { _Category = value; OnPropertyChanged(); }
        }

        private string _channelImage;
        [GTWWin8.Database.Ignore]
        public string channelImage
        {
            get { return _channelImage; }
            set { _channelImage = value ; OnPropertyChanged(); }
        }

        private string _currentStream;
        [GTWWin8.Database.Ignore]
        public string currentStream
        {
            get { return _currentStream; }
            set { _currentStream = value; OnPropertyChanged(); }
        }
        private bool _canListStreams;
        [GTWWin8.Database.Ignore]
        public bool CanListStreams
        {
            get { return _canListStreams; }
            set { _canListStreams = value; }
        }

        private ObservableCollection<ChannelStreamProgram> _allCurrentPrograms;
        [GTWWin8.Database.Ignore]
        public ObservableCollection<ChannelStreamProgram> AllCurrentPrograms
        {
            get { return _allCurrentPrograms; }
            set { _allCurrentPrograms = value; OnPropertyChanged(); }
        }
        private bool _isSpecial;

        public bool IsSpecial
        {
            get { return _isSpecial; }
            set { _isSpecial = value; }
        }
        
        
        
        private bool _favorite;

        public bool Favorite
        {
            get { return _favorite; }
            set 
            {
                _favorite = value;
                OnPropertyChanged();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propName));
        }
    }
}
