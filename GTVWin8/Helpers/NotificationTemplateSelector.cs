using GTVWin8.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GTVWin8.Helpers
{
    public class NotificationTemplateSelector : DataTemplateSelector
    {
        public DataTemplate warningTemplate { get; set; }
        public DataTemplate infoTemplate { get; set; }
        public DataTemplate channelPlusTemplate { get; set; }
        public DataTemplate channelMinusTemplate { get; set; }
        public DataTemplate dangerTemplate { get; set; }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
           switch((item as Notification).notifyType)
           {
               case NotificationType.ChannelPlus:
                   return channelPlusTemplate;
               case NotificationType.ChannelMinus:
                   return channelMinusTemplate;
               case NotificationType.Danger:
                   return dangerTemplate;
               case NotificationType.Information:
                   return infoTemplate;
               case NotificationType.Warning:
                   return warningTemplate;
               default:
                   return infoTemplate;
           }
        }
    }
}
