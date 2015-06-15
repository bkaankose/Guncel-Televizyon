using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GTVWinPhone8.DataModels
{
    public class Synchronization
    {
        [AutoIncrement , PrimaryKey]
        public int Id { get; set; }
        public DateTime SyncTime { get; set; }
    }
}
