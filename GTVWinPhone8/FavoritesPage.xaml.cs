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
using System.Collections.ObjectModel;

namespace GTVWinPhone8
{
    public partial class FavoritesPage : PhoneApplicationPage
    {
        public FavoritesPage()
        {
            InitializeComponent();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var favoriteChannelIds = await MainPage.appCore.getAllFavoriteChannels();
            ObservableCollection<Channels> favoriteChannels = new ObservableCollection<Channels>();
            foreach (var favChannel in favoriteChannelIds) favoriteChannels.Add(MainPage.appCore.allChannels.FirstOrDefault(a => a.Id == favChannel.channelId));
            if (favoriteChannels.Count != 0)
                list_Favorites.ItemsSource = favoriteChannels;
            else
                lblNoChannel.Visibility = System.Windows.Visibility.Visible;
        }
    }
}