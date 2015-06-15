using GTVWin8.DataModels;
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

namespace GTVWin8.UserControls
{
    public sealed partial class FavoritesUserControl : UserControl
    {
        GTVCore appCore;
        public FavoritesUserControl(GTVCore _appCore)
        {
            appCore = _appCore;
            this.InitializeComponent();
        }
        private void programGrid_ItemClick(object sender, ItemClickEventArgs e)
        {
            appCore.SelectedChannel = (Channels)e.ClickedItem;
        }
    }
}
