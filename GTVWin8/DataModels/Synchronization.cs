using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GTVWin8.DataModels
{
    public class Synchronization
    {
        [GTWWin8.Database.AutoIncrement , GTWWin8.Database.PrimaryKey]
        public int Id { get; set; }
        public DateTime SyncTime { get; set; }
    }
}
