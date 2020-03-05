using System.Collections.Generic;

namespace ClientModel.DeviceConfig.Request.DeviceConfig.Switches
{
    public class DeviceConfigSwitchesRequest : DeviceConfigRequestBase
    {
        public List<DeviceConfigSingleStateSwitchRequest> SingleStateSwitches { get; set; }
        public List<DeviceConfigDualStateSwitchRequest> DualStateSwitches { get; set; }
    }
}