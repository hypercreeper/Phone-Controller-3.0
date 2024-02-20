using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phone_Controller_3._0.Models
{
    class PairDeviceInfo
    {
        public PairDeviceInfo(string IP, string Port, string Code)
        {
            this.IP = IP;
            this.Port = Port;
            this.Code = Code;
        }
        public string IP
        {
            get; set;
        }
        public string Port
        {
            get; set;
        }
        public string Code
        {
            get; set;
        }
    }
}
