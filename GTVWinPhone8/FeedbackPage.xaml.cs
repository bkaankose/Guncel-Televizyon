using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace GTVWinPhone8
{
    public partial class FeedbackPage : PhoneApplicationPage
    {
        public FeedbackPage()
        {
            InitializeComponent();
        }
        public static bool IsSuccess;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemTray.ProgressIndicator = new ProgressIndicator();
            SystemTray.ProgressIndicator.Text = "Iletiliyor ...";
            IsSuccess = true;
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator.IsIndeterminate = true;
            btnSend.IsEnabled = false;
            var res = await MainPage.appCore.postMail(txtSender.Text.Trim(), txtComment.Text.Trim());
            if (res)
            {
                IsSuccess = true;
                NavigationService.GoBack();
            }
            else
                MainPage.appCore.showToast("Başarısız :(","Mesajınız iletilemedi , tekrar deneyiniz ...",0);

            SystemTray.ProgressIndicator.IsIndeterminate = false;
        }
    }
}