using Infrastructure.Common.DeviceSettings.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.DeviceConfig.Request.DeviceConfig.AssetSecurity
{
    public class DeviceConfigAssetSecurityRequest : DeviceConfigRequestBase
    {
        [JsonProperty("securityMode", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SecurityMode { get; set; }
        [JsonProperty("securityStatus", NullValueHandling = NullValueHandling.Ignore)]
        public AssetSecurityStatus? SecurityStatus { get; set; }
    }
}
