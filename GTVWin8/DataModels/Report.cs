using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTVWin8.DataModels
{
    public class Report
    {
            //        dd.Add("Name", this.allChannels.FirstOrDefault(x => x.Id == Id).Name);
            //dd.Add("channelId", Id.ToString());
            //dd.Add("reportDate", DateTime.Now.ToString());
        public string Name { get; set; }
        public int channelId { get; set; }
        public DateTime reportDate { get; set; }
    }
}
