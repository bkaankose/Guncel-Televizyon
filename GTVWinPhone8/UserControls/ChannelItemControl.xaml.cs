using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GTVWinPhone8.DataModels;

namespace GTVWinPhone8.UserControls
{
    public partial class ChannelItemControl : UserControl
    {
        public ChannelItemControl()
        {
            InitializeComponent();
        }
        private void ChannelStarTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var sent = this.DataContext as Channels;
            sent.Favorite = !sent.Favorite;
            if (sent.Favorite)
                MainPage.appCore.saveAsFavorite(sent.Id);
            else
                MainPage.appCore.removeFromFavorites(sent.Id);
        }
        
        private void StreamListTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var sent = this.DataContext as Channels;
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/StreamsPage.xaml?Id=" + sent.Id, UriKind.Relative));        }

        private void channelBorder_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var sent = this.DataContext as Channels;
            (Application.Current.RootVisual as PhoneApplicationFrame).Navigate(new Uri("/WatchPage.xaml?Id=" + sent.Id, UriKind.Relative));
        }
    }
}
