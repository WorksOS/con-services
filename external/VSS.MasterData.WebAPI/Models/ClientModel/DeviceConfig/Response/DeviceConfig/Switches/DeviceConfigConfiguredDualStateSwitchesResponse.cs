using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.Switches
{
	public class DeviceConfigConfiguredDualStateSwitchesResponse : BaseResponse<DeviceConfigConfiguredDualStateSwitchInfo, AssetErrorInfo>
    {
        public DeviceConfigConfiguredDualStateSwitchesResponse() { }

        public DeviceConfigConfiguredDualStateSwitchesResponse(IList<AssetErrorInfo> errors) : base(errors) { }

        public DeviceConfigConfiguredDualStateSwitchesResponse(AssetErrorInfo errors) : base(errors) { }

        public DeviceConfigConfiguredDualStateSwitchesResponse(IEnumerable<DeviceConfigConfiguredDualStateSwitchInfo> switchesWithConfiguredValues, IList<AssetErrorInfo> errors = null) : base(switchesWithConfiguredValues, errors) { }

        [JsonProperty("deviceConfigSwitches", NullValueHandling = NullValueHandling.Ignore)]
        public override IEnumerable<DeviceConfigConfiguredDualStateSwitchInfo> Lists { get; set; }

    }
}
