using GTVWin8.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace GTVWin8
{
    public static class GTVSettings
    {
        public static double GetVolume()
        {
            return App.DatabaseConnection.QueryAsync<Settings>("SELECT * FROM Settings").Result[0].Volume;
        }
        public static bool GetHD()
        {
            return App.DatabaseConnection.QueryAsync<Settings>("SELECT * FROM Settings").Result[0].IsHD;
        }
        public static async void SetHD(bool isHD)
        {
            await App.AsyncDatabaseConnection.ExecuteAsync("UPDATE Settings SET IsHD = " + Convert.ToInt32(isHD));
        }
        public static async void SetVolume(double Val)
        {
            await App.AsyncDatabaseConnection.ExecuteAsync("UPDATE Settings SET Volume = " + (int)Val);
        }
    }
}
