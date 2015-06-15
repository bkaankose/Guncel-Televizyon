using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace GTVWin8.UserControls
{
    public sealed partial class NotificationsUserControl : UserControl
    {
        private GTVCore appCore;
        public NotificationsUserControl(GTVCore _appCore)
        {
            this.InitializeComponent();
            this.Loaded += NotificationsUserControl_Loaded;
            appCore = _appCore;
        }
        private void NotificationsUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            notificationsList.ItemsSource = appCore.allNotifications;
        }
    }
}
