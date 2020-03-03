using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.DeviceSettings
{
    public class DeviceConfigMessage
    {
        public ParamGroup Group { get; set; }
        public TimestampDetail Timestamp { get; set; }
    }
}
