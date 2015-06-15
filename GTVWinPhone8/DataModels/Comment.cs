using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTVWinPhone8.DataModels
{
    public class Comment
    {
        public string Sender { get; set; }
        public string Content { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
