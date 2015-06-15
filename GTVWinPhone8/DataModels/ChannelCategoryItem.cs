using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GTVWinPhone8.DataModels
{
    public class CategoryItem : INotifyPropertyChanged
    {
        public string iconPath { get; set; }
        public string menuText { get; set; }
        private int channelCount;

        public int ChannelCount
        {
            get { return channelCount; }
            set { 
                channelCount = value;
                NotifyPropertyChanged("ChannelCount");
            }
        }
        
        public Category cCategory { get; set; }

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
