using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace admin_client.Model
{
    public class Log
    {
        public string TargetId { get; set; }
        public string Msg { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public string DetectedAt { get; set; }
    }
}
