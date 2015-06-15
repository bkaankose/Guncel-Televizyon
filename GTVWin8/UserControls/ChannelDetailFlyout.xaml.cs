using GTVWin8.DataModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace GTVWin8.UserControls
{
    public sealed partial class ChannelDetailFlyout : SettingsFlyout
    {
        private GTVCore appCore;
        public event EventHandler PlayClicked;
        public static int latestLoadedId = -1;
        public ChannelDetailFlyout(GTVCore _gtvCore)
        {
            this.InitializeComponent();
            appCore = _gtvCore;
            this.Loaded += ChannelDetailFlyout_Loaded;
        }

        async void ChannelDetailFlyout_Loaded(object sender, RoutedEventArgs e)
        {
            if(appCore.SelectedChannel.CanListStreams)
                if (appCore.SelectedChannel.Id != latestLoadedId)
                    if(appCore.SelectedChannel.AllCurrentPrograms.Count == 0)
                    {
                        var latestLoadedStreams = await appCore.getChannelStream(appCore.SelectedChannel.Id);
                        if (latestLoadedStreams == null) return;
                        foreach (var item in latestLoadedStreams) appCore.SelectedChannel.AllCurrentPrograms.Add(item);
                        latestLoadedId = appCore.SelectedChannel.Id;
                    }
        }

        private void btnFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (appCore.SelectedChannel.Favorite)
                appCore.removeFromFavorites(appCore.SelectedChannel.Id);
            else
                appCore.saveAsFavorite(appCore.SelectedChannel.Id);
        }
        
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            appCore.navigateToWatch = true;
            PlayClicked(btnPlay, new EventArgs());
            this.Hide();
        }
        
    }
}
