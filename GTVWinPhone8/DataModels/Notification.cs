using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTVWinPhone8.DataModels
{
    public enum NotificationType
    {
        ChannelPlus,// New channel inserted
        ChannelMinus, // Old channel deleted
        Information, // Channels updated
        Warning, // Something important
        Danger // Something critical
    }
    public class Notification
    {
        public DateTime notifyDate { get; set; }
        public string notifyContent { get; set; }
        public NotificationType notifyType { get; set; }
        private bool _isRead = true;

        public bool IsRead
        {
            get { return _isRead; }
            set { _isRead = value; }
        }
        
    }
}
