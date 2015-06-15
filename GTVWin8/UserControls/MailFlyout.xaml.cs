using GTVWin8.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace GTVWin8.UserControls
{
    public sealed partial class MailFlyout : SettingsFlyout
    {
        GTVCore appCore;
        public MailFlyout(GTVCore _appCore)
        {
            appCore = _appCore;
            this.InitializeComponent();
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtComment.Text) ||string.IsNullOrEmpty(txtName.Text)) return;
            btnSend.IsEnabled = false;

            var res = await appCore.postMail(txtName.Text.Trim(), txtComment.Text.Trim());
            string msgParam = string.Empty;
            if (res)
            {
                msgParam = "Mesajınız iletildi !";
                MessageDialog msg = new MessageDialog(msgParam);
                await msg.ShowAsync();
            }
            this.Hide();
        }
        private void txtComment_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblCount.Text = (255 - txtComment.Text.Count()).ToString();
        }

    }
}
