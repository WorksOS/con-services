using System.Collections.Generic;
using CommonModel.Error;
using Newtonsoft.Json;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.Switches
{
    public class DeviceConfigSwitchesResponse : BaseResponse<DeviceConfigSwitches, AssetErrorInfo>
    {
        public DeviceConfigSwitchesResponse()
        {

        }

        public DeviceConfigSwitchesResponse(IList<AssetErrorInfo> errors) : base(errors) { }

        public DeviceConfigSwitchesResponse(AssetErrorInfo errors) : base(errors) { }

        public DeviceConfigSwitchesResponse(IEnumerable<DeviceConfigSwitches> switchesWithConfiguredValues, IList<AssetErrorInfo> errors = null) : base(switchesWithConfiguredValues, errors) { }

        [JsonProperty("deviceConfigSwitches", NullValueHandling = NullValueHandling.Ignore)]
        public override IEnumerable<DeviceConfigSwitches> Lists { get; set; }
    }
}
