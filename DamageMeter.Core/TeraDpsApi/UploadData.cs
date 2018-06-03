using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageMeter.TeraDpsApi
{
    public struct UploadData
    {
        public bool Success { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }
        public string Npc { get; set; }
        public string Server { get; set; }
        public string Exception { get; set; }
        public string Url { get; set; }
    }
}
