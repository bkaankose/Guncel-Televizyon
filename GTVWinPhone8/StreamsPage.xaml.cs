using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using GTVWinPhone8.DataModels;

namespace GTVWinPhone8
{
    public partial class StreamsPage : PhoneApplicationPage
    {
        private class StreamsPageViewModel
        {
            public string ChannelName { get; set; }
            public Uri ChannelImageURL { get; set; }
            public string CurrentStream { get; set; }
            public ObservableCollection<ChannelStreamProgram> AllStreams { get; set; }
            public StreamsPageViewModel(string _channelName,Uri _channelImageUrl)
            {
                ChannelName = _channelName;
                ChannelImageURL = _channelImageUrl;
            }
        }
        StreamsPageViewModel StreamsVM;
        public StreamsPage()
        {
            InitializeComponent();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string cName,cId;
            Uri cImage;
            if (NavigationContext.QueryString.TryGetValue("Id", out cId))
                StreamsVM = new StreamsPageViewModel(MainPage.appCore.allChannels.FirstOrDefault(x => x.Id == Int32.Parse(cId)).Name, new Uri(MainPage.appCore.allChannels.FirstOrDefault(a => a.Id == Int32.Parse(cId)).channelImage, UriKind.Absolute));
            else
            {
                NavigationService.GoBack();
                return;
            }

            StreamsVM.CurrentStream = MainPage.appCore.allChannels.FirstOrDefault(a => a.Id == Int32.Parse(cId)).currentStream;
            StreamsVM.AllStreams = await MainPage.appCore.getChannelStream(Int32.Parse(cId));
            this.DataContext = StreamsVM;
            DisableLoading();
        }
        private void DisableLoading()
        {
            grid_Loading.Visibility = System.Windows.Visibility.Collapsed;
            grid_Info.Visibility = System.Windows.Visibility.Visible;
        }
    }
}