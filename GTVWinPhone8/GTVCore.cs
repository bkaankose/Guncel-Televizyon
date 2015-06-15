using Coding4Fun.Toolkit.Controls;
using GTVWinPhone8.DataModels;
using GTVWinPhone8.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GTVWinPhone8
{
    public class GTVCore : IGTVCore
    {
        GTVService appService;
        public ObservableCollection<Channels> allChannels;
        public GTVCore()
        {
            appService = new GTVService();
        }
        public async void removeFromFavorites(int Id)
        {
            await App.DatabaseConnection.ExecuteAsync("DELETE FROM Favorites WHERE channelId=?", Id);
        }

        public async void saveAsFavorite(int Id)
        {
            await App.DatabaseConnection.InsertAsync(new Favorites() { channelId = Id });
        }

        public System.Collections.ObjectModel.ObservableCollection<DataModels.Channels> searchChannels(string sText)
        {
            throw new NotImplementedException();
        }

        public Task<System.Collections.ObjectModel.ObservableCollection<DataModels.Notification>> getNotifications()
        {
            throw new NotImplementedException();
        }

        public async Task<System.Collections.ObjectModel.ObservableCollection<DataModels.Channels>> getChannels()
        {
            try
            {
                var res = await appService.getFromAPI("Channels");
                if (!res.StatusValid) return null;
                await App.DatabaseConnection.InsertAsync(new Synchronization() { SyncTime = DateTime.Now });
                return new ObservableCollection<Channels>(JsonConvert.DeserializeObject<ObservableCollection<Channels>>(JArray.Parse(res.StatusMessage).ToString()));
            }
            catch { return null; }
        }

        public async Task<System.Collections.ObjectModel.ObservableCollection<DataModels.StreamProgram>> getWhatToWatch()
        {
            try
            {
                var res = await appService.getFromAPI("Streams?whatToWatch&Time=" + DateTime.Now.Hour + ":" + DateTime.Now.Minute);
                if (!res.StatusValid) return null;
                return new ObservableCollection<StreamProgram>(JsonConvert.DeserializeObject<ObservableCollection<StreamProgram>>(JArray.Parse(res.StatusMessage).ToString()));
            }
            catch { return null; }
        }

        public async Task<System.Collections.ObjectModel.ObservableCollection<DataModels.ChannelStreamProgram>> getChannelStream(int Id)
        {
            try
            {
                var res = await appService.getFromAPI("Streams/getChannelStream?Id=" + Id);
                if (!res.StatusValid) return null;
                return new ObservableCollection<ChannelStreamProgram>(JsonConvert.DeserializeObject<ObservableCollection<ChannelStreamProgram>>(JArray.Parse(res.StatusMessage).ToString()));
            }
            catch { return null; }
        }

        public async Task<List<DataModels.Favorites>> getAllFavoriteChannels()
        {
            var res = await App.DatabaseConnection.QueryAsync<Favorites>("SELECT * FROM Favorites");
            return res;
        }

        public Task<string> getSpecialURL(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> postReport(int Id)
        {
            throw new NotImplementedException();
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
        public void showToast(string Title,string Text,int Seconds)
        {
            var toast = new ToastPrompt() { Message = Text, Title = Title, TextOrientation = System.Windows.Controls.Orientation.Vertical, Background = new SolidColorBrush(Colors.White), Foreground = new SolidColorBrush(Color.FromArgb(255, 3, 166, 120)), FontSize = 20 };
            toast.Show();
        }
        public int checkLocalVersion()
        {
            return App.VERSION;
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
                var r2 = (await App.DatabaseConnection.QueryAsync<Synchronization>("SELECT * FROM Synchronization ORDER BY SyncTime DESC")).FirstOrDefault();
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
