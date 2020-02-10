using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.DeviceConfig.Request.DeviceConfig
{
    public class PendingDeviceConfigRequest
    {
        public IList<AssetDeviceConfigRequestDto> PendingDeviceConfigs { get; set; }
    }
}
