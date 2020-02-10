using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbModel.DeviceConfig
{
   public class DbDevice
    {
        public string DeviceUID { get; set; }

        public string DeviceSerialNumber { get; set; }

        public string DeviceType { get; set; }

        public string DeviceState { get; set; }

        public DateTime? DeregisteredUTC { get; set; }

        public string ModuleType { get; set; }

        public string DataLinkType { get; set; }
    }
}
