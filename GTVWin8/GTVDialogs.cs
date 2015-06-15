using GTVWin8.DataModels;
using GTVWin8.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.Connectivity;
using Windows.UI.Popups;

namespace GTVWin8
{
    public static class GTVDialogs
    {
        private static GTVService GTVService = new GTVService();
        public static IAsyncOperation<IUICommand> ShowErrorAsync(string error)
        {
            MessageDialog dialog = null;
            if(IsInternet())
            {
                dialog = new MessageDialog("Internet bağlantınız mevcut , fakat uygulama bir hata ile karşılaştı.\nHata raporu göndererek düzeltilmesi için yardımcı olabilirsiniz.  ", "Beklenmedik bir hata oluştu :(");
                dialog.Commands.Add(new UICommand("Hata Raporu Gönder", o => ReportError(error)));
            }
            else
                dialog = new MessageDialog("Internet bağlantınız yok.Uygulama bu şekilde çalışmaya devam edemez :/", "Beklenmedik bir hata oluştu :(");
                
            dialog.Commands.Add(new UICommand("Uygulamayı Kapat", o => App.Current.Exit()));
            return dialog.ShowAsync();
        }
        public static async Task ShowDialog(string Message,string Title)
        {
            var msg = new MessageDialog(Message,Title);
            await msg.ShowAsync();
        }
        public static async void ReportError(string errMessage)
        {
            var nRet = new AppReport() { CreationTime = DateTime.Now , Report = errMessage };
            var res = await GTVService.postToAPI("AppReports", nRet);
            if (res)
                await ShowDialog("Çökme raporu başarıyla iletildi ! Teşekkür ederiz.","Ileti Raporu");
            else
                await ShowDialog("Çökme raporu gönderilirken hata oluştu , daha sonra tekrar deneyiniz :(", "Ileti Raporu");
        }
        public static bool IsInternet() 
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return internet;
        }
    }
}
