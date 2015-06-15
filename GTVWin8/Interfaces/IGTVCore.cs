using GTVWin8.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace GTVWin8.Interfaces
{
    public interface IGTVCore
    {
         
        void removeFromFavorites(int Id);
        void saveAsFavorite(int Id);
        ObservableCollection<Channels> searchChannels(string sText);
        Task<ObservableCollection<Notification>> getNotifications();
        Task<ObservableCollection<Channels>> getChannels();
        Task<ObservableCollection<StreamProgram>> getWhatToWatch();
        Task<ObservableCollection<ChannelStreamProgram>> getChannelStream(int Id);
        Task<List<Favorites>> getAllFavoriteChannels();
        Task<string> getSpecialURL(int Id);
        Task<bool> postReport(int Id);
        Task<bool> postMail(string sender, string content);
        int checkLocalVersion();
        Task<bool> checkRemoteVersion(int localVersion);
        
    }
}

