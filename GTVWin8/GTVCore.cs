using GTVWin8.DataModels;
using GTVWin8.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GTVWin8
{
    public class GTVCore : IGTVCore
    {
        public ObservableCollection<Channels> allChannels;
        public ObservableCollection<Channels> tmpAllChannels;
        public ObservableCollection<Notification> allNotifications;
        protected GTVService appService;
        public bool navigateToWatch, favChanged;
        private MenuFlyout favoritesFlyout = null;
        public DateTime latestNotification = Convert.ToDateTime(App.localSettings.Values["latestNotification"]);
        public Channels SelectedChannel { get; set; }
        public event RoutedEventHandler favoriteClicked;
        public GTVCore()
        {
            allChannels = new ObservableCollection<Channels>();
            allNotifications = new ObservableCollection<Notification>();
            appService = new GTVService();
            favoritesFlyout = new MenuFlyout();
            favChanged = true;
        }
        public async void removeFromFavorites(int Id)
        {
            await App.AsyncDatabaseConnection.ExecuteAsync("DELETE FROM Favorites WHERE channelId=?", Id);
            allChannels.First(x => x.Id == Id).Favorite = false;
            favoritesFlyout.Items.Remove(new MenuFlyoutItem() { Text = allChannels.First(x => x.Id == Id).Name });
            favChanged = true;
        }
        public async Task<List<Favorites>> getAllFavoriteChannels()
        {
            var res = await App.AsyncDatabaseConnection.QueryAsync<Favorites>("SELECT * FROM Favorites");
            return res;
        }
        public ObservableCollection<Channels> searchChannels(string sText)
        {
            return new ObservableCollection<Channels>(allChannels.Where(x => x.Name.Contains(sText.Trim()) || x.currentStream.Contains(sText.Trim())).ToList());
        }
        public async void saveAsFavorite(int Id)
        {
            await App.AsyncDatabaseConnection.InsertAsync(new Favorites() { channelId = Id });
            allChannels.First(x => x.Id == Id).Favorite = true;
            favoritesFlyout.Items.Add(new MenuFlyoutItem() { Text = allChannels.First(x => x.Id == Id).Name });
            favChanged = true;
        }
        public MenuFlyout getFavFlyout()
        {
            if(favChanged)
            {
                favoritesFlyout.Items.Clear();
                foreach (var ch in allChannels.Where(x => x.Favorite == true))
                {
                    var tmpMItem = new MenuFlyoutItem() { Text = ch.Name };
                    tmpMItem.Click += favoriteClicked;
                    favoritesFlyout.Items.Add(tmpMItem);
                }
            }
            favChanged = false;
            return favoritesFlyout;
        }
        public async Task<bool> checkLocalPic(string channelName)
        {
            bool found = false;
            try
            {
                var cPic = await App.localPics.GetFileAsync("ChannelPics\\" + channelName + ".png");
                found = true;
            }
            catch { }
            return found;
        }
        public async Task<ObservableCollection<Notification>> getNotifications()
        {
            var notifies = await App.AsyncDatabaseConnection.QueryAsync<Notification>("SELECT * FROM Notifications");
            return new ObservableCollection<Notification>(notifies);
        }
        public async Task<string> getSpecialURL(int Id)
        {
            try
            {
                var res = await appService.getFromAPI("SpecialUri?SpecialUri&Id=" + Id);
                if (!res.StatusValid) return null;
                return res.StatusMessage;
            }
            catch { return null; }
        }
        public async Task<ObservableCollection<Channels>> getChannels()
        {
            try
            {
                var res = await appService.getFromAPI("Channels");
                if (!res.StatusValid) return null;
                await App.AsyncDatabaseConnection.InsertAsync(new Synchronization() { SyncTime = DateTime.Now });
                return new ObservableCollection<Channels>(JsonConvert.DeserializeObject<ObservableCollection<Channels>>(JArray.Parse(res.StatusMessage).ToString()));
            }
            catch { return null;  }
        }

        public async Task<ObservableCollection<StreamProgram>> getWhatToWatch()
        {
            try
            {
                var res = await appService.getFromAPI("Streams?whatToWatch&Time=" + DateTime.Now.Hour + ":" + DateTime.Now.Minute);
                if (!res.StatusValid) return null;
                return new ObservableCollection<StreamProgram>(JsonConvert.DeserializeObject<ObservableCollection<StreamProgram>>(JArray.Parse(res.StatusMessage).ToString()));
            }
            catch { return null;  }
            
        }
        public async Task<ObservableCollection<ChannelStreamProgram>> getChannelStream(int Id)
        {
            try
            {
                var res = await appService.getFromAPI("Streams/getChannelStream?Id=" + Id);
                if (!res.StatusValid) return null;
                return new ObservableCollection<ChannelStreamProgram>(JsonConvert.DeserializeObject<ObservableCollection<ChannelStreamProgram>>(JArray.Parse(res.StatusMessage).ToString()));
            }
            catch { return null; }
        }

        public async Task<int?> checkChannelSync()
        {
            /* 0 : Sync not required
             * 1 : Sync requied
             * null : Failed to connect */
            try
            {
                var res = await appService.getFromAPI("ChannelSyncHistories");
                if (!res.StatusValid) return null;
                var r1 = JsonConvert.DeserializeObject<Synchronization>(JObject.Parse(res.StatusMessage).ToString());
                var r2 = (await App.AsyncDatabaseConnection.QueryAsync<Synchronization>("SELECT * FROM Synchronization ORDER BY SyncTime DESC")).FirstOrDefault();
                if (r2 != null)
                    if (r2.SyncTime <= r1.SyncTime)
                        return 1;
                    else
                        return 0;
                else
                    return 1;
            }
            catch { return null; }
        }

        public async Task<bool> postReport(int Id)
        {
            var nret = new Report() { reportDate = DateTime.Now, channelId = Id, Name = allChannels.First(x => x.Id == Id).Name };
            try
            {
                var res = await appService.postToAPI("Reports", nret);
                return res;
            }
            catch { return false; }
        }

        public async Task<bool> postMail(string sender, string content)
        {
            var nRet = new Comment() { CreationTime = DateTime.Now, Content = content, Sender = sender };
            try
            {
                var res = await appService.postToAPI("Comments", nRet);
                return res;
            }
            catch { return false; }
        }
        public int checkLocalVersion()
        {
            //var res = await App.AsyncDatabaseConnection.QueryAsync<int>("SELECT versionId FROM Version ORDER BY versionId ASC");
            //return res.LastOrDefault();
            return App.VERSION;
        }

        public async Task<bool> checkRemoteVersion(int localVersion)
        {
            try
            {
                var res = await appService.getFromAPI("Versions?VersionCheck&versionId=" + localVersion);
                if (!res.StatusValid) return false;
                return bool.Parse(res.StatusMessage);
            }
            catch { return false; }
        }
    }
}
