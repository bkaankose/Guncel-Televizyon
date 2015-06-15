using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Net.Http;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GTVWin8.DataModels;
using GTVWin8.UserControls;
using GTVWin8.Interfaces;
using GTVWin8.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.System;
using Windows.ApplicationModel;
using Windows.UI;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Syncfusion.UI.Xaml.Controls.Navigation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace GTVWin8
{
    public sealed partial class MainPage : Page
    {
        private static ObservableCollection<Channels> tempAllChannels = new ObservableCollection<Channels>();
        private static GTVCore appCore = new GTVCore();
        private MainPageViewModel MainModel = new MainPageViewModel();
        public static IEnumerable<IGrouping<Category, Channels>> queries;
        
        public MainPage()
        {
            this.InitializeComponent();
            /* Load categories */
            menuView.Items.Add(new CategoryItem() { menuText = "Tüm Kanallar", iconPath = "/Images/MenuView/TumKanallar.png" });
            menuView.Items.Add(new CategoryItem() { menuText = "Ulusal", iconPath = "/Images/MenuView/Ulusal.png" , cCategory = Category.Ulusal });
            menuView.Items.Add(new CategoryItem() { menuText = "Haber", iconPath = "/Images/MenuView/Haber.png", cCategory = Category.Haber });
            menuView.Items.Add(new CategoryItem() { menuText = "Spor", iconPath = "/Images/MenuView/Spor.png", cCategory = Category.Spor });
            menuView.Items.Add(new CategoryItem() { menuText = "Müzik", iconPath = "/Images/MenuView/Müzik.png", cCategory = Category.Müzik });
            menuView.Items.Add(new CategoryItem() { menuText = "Sinema", iconPath = "/Images/MenuView/Sinema.png", cCategory = Category.Sinema });
            menuView.Items.Add(new CategoryItem() { menuText = "Belgesel", iconPath = "/Images/MenuView/Belgesel.png", cCategory = Category.Belgesel });
            menuView.Items.Add(new CategoryItem() { menuText = "Çocuk", iconPath = "/Images/MenuView/Çocuk.png", cCategory = Category.Çocuk });
            menuView.Items.Add(new CategoryItem() { menuText = "Eğlence", iconPath = "/Images/MenuView/Eğlence.png", cCategory = Category.Eğlence });
            menuView.Items.Add(new CategoryItem() { menuText = "Moda", iconPath = "/Images/MenuView/Moda.png", cCategory = Category.Moda });
            menuView.SelectedIndex = 0;
            menuView.Focus(FocusState.Pointer);

            this.DataContext = MainModel;
            menuView.SelectionChanged += menuView_SelectionChanged;
            appCore.favoriteClicked += appCore_favoriteClicked;
            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
        }
        async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                MainModel.IsNetworkAvailable = GTVDialogs.IsInternet();
            });
        }
        void menuView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if((sender as ListView).SelectedIndex == 0) // Tüm Kanallar
                tempAllChannels = appCore.allChannels;
            else
                tempAllChannels = new ObservableCollection<Channels>(appCore.allChannels.Where(x => x.Category.ToString() == ((sender as ListView).SelectedItem as CategoryItem).menuText));

            queries = from item in tempAllChannels
                      orderby item.Category
                      group item by item.Category into g
                      select g;

            this.cvs.Source = queries;
            setNoChannel();
        }
        void setNoChannel()
        {
            if (queries.Count() == 0)
                lblNoChannel.Visibility = Visibility.Visible;
            else
                lblNoChannel.Visibility = Visibility.Collapsed;
        }
        private void appCore_favoriteClicked(object sender, RoutedEventArgs e)
        {
            appCore.SelectedChannel = appCore.allChannels.FirstOrDefault(x => x.Name == ((dynamic)sender).Text);
            Frame.Navigate(typeof(WatchPage), appCore);
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NetworkInformation_NetworkStatusChanged(this);
            if (e.NavigationMode == Windows.UI.Xaml.Navigation.NavigationMode.Back)
            {
                queries = from item in appCore.allChannels.OrderBy(a => a.Name)
                          orderby item.Category
                          group item by item.Category into g
                          select g;

                foreach (var mItem in menuView.Items) { ((CategoryItem)mItem).ChannelCount = appCore.allChannels.Where(a => a.Category == ((CategoryItem)mItem).cCategory).Count(); }
                ((CategoryItem)menuView.Items[0]).ChannelCount = appCore.allChannels.Count;

                cvs.Source = queries;
                menuView.SelectedIndex = 0;
                menuView.IsEnabled = true;
                txtSearch.IsEnabled = true;
                setProgress(false);
                menuView.Focus(FocusState.Pointer);
                return;
            }
            /* Check version */
            setProgress(true,"Sürüm kontrol ediliyor ...");
            
            var updateNeeded = await appCore.checkRemoteVersion(appCore.checkLocalVersion());
            if (updateNeeded)
                await GTVDialogs.ShowDialog("Güncel Televizyonun daha yeni bir sürümü markette mevcuttur.\nYeni özellikleri ve düzeltmeleri kullanabilmek için lütfen uygulamanızı güncelleyiniz !","Eski Sürümü Kullanıyorsunuz !");

            setProgress(true,"Kanallar Yükleniyor ... ");
            
            var syncNeed = await appCore.checkChannelSync();
            if(syncNeed == null)
            {
                appCore.allChannels = new ObservableCollection<Channels>(await App.AsyncDatabaseConnection.QueryAsync<Channels>("SELECT * FROM Channels"));
            }
            else if (syncNeed == 1)
            {
                appCore.allChannels = await appCore.getChannels();
                await App.AsyncDatabaseConnection.ExecuteAsync("DELETE FROM Channels");
                await App.AsyncDatabaseConnection.InsertAllAsync(appCore.allChannels);
            }else if(syncNeed == 0)
                appCore.allChannels = new ObservableCollection<Channels>(await App.AsyncDatabaseConnection.QueryAsync<Channels>("SELECT * FROM Channels"));

            foreach (var mItem in menuView.Items) { ((CategoryItem)mItem).ChannelCount = appCore.allChannels.Where(a => a.Category == ((CategoryItem)mItem).cCategory).Count(); }

            ((CategoryItem)menuView.Items[0]).ChannelCount = appCore.allChannels.Count;

            await loadImages();

            //* Load notifications */
            //appCore.allNotifications = await appCore.getNotifications();
            //if (tempN != null) appCore.allNotifications = tempN;
            //foreach (var notify in appCore.allNotifications)
            //    if (notify.notifyDate.AddHours(2) >= appCore.latestNotification)
            //        notify.IsRead = false;

            /* Load current streams */
            var streamContext = await appCore.getWhatToWatch();
            if (streamContext != null)
                foreach (var streamContent in streamContext)
                    appCore.allChannels.FirstOrDefault(a => a.Id == streamContent.Id).currentStream = streamContent.streamContent;

            foreach (var channel in appCore.allChannels)
                if (channel.currentStream == null)
                    channel.currentStream = channel.Name;
                else
                    channel.CanListStreams = true;

            /* Load favorite channels */
            foreach (var chn in await appCore.getAllFavoriteChannels()) appCore.allChannels.FirstOrDefault(a => a.Id == chn.channelId).Favorite = true;
            
            ///* Group channels by categories */
            queries = from item in appCore.allChannels.OrderBy(a => a.Name).Where(x => x.IsActive)
                      orderby item.Category
                      group item by item.Category into g
                      select g;

            cvs.Source = queries;
            setProgress(false);
            setNoChannel();
            menuView.IsEnabled = true;
            txtSearch.IsEnabled = true;
        }
        private async Task loadImages()
        {
            foreach (var chn in appCore.allChannels)
            {
                if (!await appCore.checkLocalPic(chn.Name))
                {
                    using (var wc = new HttpClient())
                    {
                        setProgress(true, "Kanal yükleniyor (" + chn.Name + ")");
                        try
                        {
                            var img = await wc.GetByteArrayAsync(App.StorageUrl + chn.Name + ".png");
                            var imgFile = await App.localPics.CreateFileAsync("ChannelPics\\" + chn.Name + ".png", CreationCollisionOption.ReplaceExisting);
                            await FileIO.WriteBytesAsync(imgFile, img);
                        }
                        catch { }
                    }
                }
                chn.channelImage = App.localPics.Path + "\\ChannelPics\\" + chn.Name + ".png";
            }
        }
        private void setProgress(bool Running = false,string Content = "")
        {
            midProgress.IsBusy = Running;
            if (Running)
                midProgress.Content = Content;
            else
                midProgress.Content = null;

        }
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            queries = from item in appCore.searchChannels(txtSearch.Text.Trim())
                        orderby item.Category
                        group item by item.Category into g
                        select g;

            this.cvs.Source = queries;
            setNoChannel();
        }
        private void channelsGridCollection_ItemClick(object sender, Windows.UI.Xaml.Controls.ItemClickEventArgs e)
        {
            appCore.SelectedChannel = e.ClickedItem as Channels;
            var CDF = new ChannelDetailFlyout(appCore);
            CDF.PlayClicked += CDF_PlayClicked;
            CDF.DataContext = appCore.SelectedChannel;
            CDF.ShowIndependent();
        }
        void CDF_PlayClicked(object sender, EventArgs e)
        {
            if (appCore.navigateToWatch)
                Frame.Navigate(typeof(WatchPage), appCore);
        }
        private void btnFavUnFav_Click(object sender, RoutedEventArgs e)
        {
            if (appCore.SelectedChannel.Favorite) // unfav
                appCore.removeFromFavorites(appCore.SelectedChannel.Id);
            else
                appCore.saveAsFavorite(appCore.SelectedChannel.Id);
        }

        private async void btnRate_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=" + Package.Current.Id.FamilyName , UriKind.Absolute));
        }
        private void btnMail_Click(object sender, RoutedEventArgs e)
        {
            var mailFlyout = new MailFlyout(appCore);
            mailFlyout.ShowIndependent();
        }
        private void SemanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (e.SourceItem.Item == null) return;
            e.DestinationItem = new SemanticZoomLocation { Item = e.SourceItem.Item };
        }

        private void btnFavorites_Click(object sender, RoutedEventArgs e)
        {
            btnFavorites.Flyout = appCore.getFavFlyout();
        }


        //private void btnNotifications_Click(object sender, RoutedEventArgs e)
        //{
        //    if (appCore.allNotifications != null)
        //        if (appCore.allNotifications.Count == 0) return;

        //    App.localSettings.Values["latestNotification"] = DateTime.Now.ToString();
        //    Flyout fl = new Flyout();
        //    var notUC = new NotificationsUserControl(appCore);
        //    fl.Content = notUC;
        //    fl.Placement = FlyoutPlacementMode.Bottom;
        //    fl.ShowAt(sender as Button);
        //}

    }
}
