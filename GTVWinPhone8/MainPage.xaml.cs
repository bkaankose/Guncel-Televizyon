using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using GTVWinPhone8.DataModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using System.Collections.ObjectModel;
using GTVWinPhone8.ViewModels;
using System.Threading.Tasks;
using System.Net.Http;
using Windows.Storage;
using System.Diagnostics;
using Microsoft.Phone.Shell;

namespace GTVWinPhone8
{
    public partial class MainPage : PhoneApplicationPage,INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public static GTVCore appCore = new GTVCore();
        public static IEnumerable<IGrouping<Category, Channels>> queries;
        public MainPage()
        {
            
            InitializeComponent();
            this.Loaded += (c, r) =>
            {
                if (FeedbackPage.IsSuccess)
                    appCore.showToast("Başarılı !", "Mesajınız iletilmiştir ...", 0);
            };
            this.ApplicationBar.IsVisible = false;
            //(App.Current as App).rateReminder.Notify();
        }
        #region Properties
        private ObservableCollection<Channels> _mainChannels;

        public ObservableCollection<Channels> MainChannels
        {
            get { return _mainChannels; }
            set { _mainChannels = value; NotifyPropertyChanged(); }
        }
        
        #endregion
        #region Helpers
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private async Task loadImages()
        {
            foreach (var chn in appCore.allChannels)
            {
                if (!await appCore.checkLocalPic(chn.Name))
                {
                    using (var wc = new HttpClient())
                    {
                        ctrlLoading.LoadingText = chn.Name + " Yükleniyor ...";
                        try
                        {
                            var img = await wc.GetByteArrayAsync(App.StorageUrl + chn.Name + ".png");
                            var imgFile = await App.localPics.CreateFileAsync("ChannelPics\\" + chn.Name + ".png", CreationCollisionOption.ReplaceExisting);
                            using (var s = await imgFile.OpenStreamForWriteAsync())
                            {
                                s.Write(img, 0, img.Length);
                            }
                        }
                        catch { }
                    }
                }
                chn.channelImage = App.localPics.Path + "\\ChannelPics\\" + chn.Name + ".png";
            }
        }
        protected override async void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back) return;
            var updateNeeded = await appCore.checkRemoteVersion(appCore.checkLocalVersion());
            if (updateNeeded)
                MessageBox.Show("Eski versionu kullanıyorsunuz.\nLütfen Windows Mağaza'dan güncelleme yaparak yeni sürümü indiriniz !");

            var syncNeed = await appCore.checkChannelSync();
            if (syncNeed == null)
            {
                appCore.allChannels = new ObservableCollection<Channels>(await App.DatabaseConnection.QueryAsync<Channels>("SELECT * FROM Channels"));
            }
            else if (syncNeed == 1)
            {
                appCore.allChannels = await appCore.getChannels();
                await App.DatabaseConnection.ExecuteAsync("DELETE FROM Channels");
                await App.DatabaseConnection.InsertAllAsync(appCore.allChannels);
            }
            else if (syncNeed == 0)
                appCore.allChannels = new ObservableCollection<Channels>(await App.DatabaseConnection.QueryAsync<Channels>("SELECT * FROM Channels"));

            foreach (var chn in await appCore.getAllFavoriteChannels()) appCore.allChannels.FirstOrDefault(a => a.Id == chn.channelId).Favorite = true;

            await loadImages();

            var streamContext = await appCore.getWhatToWatch();
            if (streamContext != null)
                foreach (var streamContent in streamContext)
                    appCore.allChannels.FirstOrDefault(a => a.Id == streamContent.Id).currentStream = streamContent.streamContent;

            foreach (var channel in appCore.allChannels)
                if (channel.currentStream != null)
                    channel.CanListStreams = true;
            
            (mainPivot.Items[0] as PivotItem).Content = new LongListSelector() { ItemTemplate = App.Current.Resources["channelItemTemplate"] as DataTemplate, ItemsSource = appCore.allChannels };

            for(int k = 1;k <= 9;k++)
                (mainPivot.Items[k] as PivotItem).Content = new LongListSelector() { ItemTemplate = App.Current.Resources["channelItemTemplate"] as DataTemplate, ItemsSource = appCore.allChannels.Where(a => a.Category == (Category)(Int32.Parse((mainPivot.Items[k - 1] as PivotItem).Tag.ToString()))).ToList() };

            DisableLoading();
            this.ApplicationBar.IsVisible = true;
        }
        private void DisableLoading()
        {
            grid_Loading.Visibility = System.Windows.Visibility.Collapsed;
            grid_Channels.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnMail_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/FeedbackPage.xaml",UriKind.Relative));
        }

        private void btnFavorites_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/FavoritesPage.xaml", UriKind.Relative));
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }
    }
}
