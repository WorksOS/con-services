using System;
using System.Collections.Generic;

namespace ClientModel.AssetSettings.Request
{
	public class PendingDeviceConfigRequest
    {
        public List<PendingDeviceConfig> PendingDeviceConfigs { get; set; }
    }

    public class PendingDeviceConfig
    {
        public List<Guid> DeviceUIDs { get; set; }
        public string GroupName { get; set; }
        public string ParameterName { get; set; }
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
        public DateTime ActionUTC { get; set; }
        public string DeviceType { get; set; }
    }
}
