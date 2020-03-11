using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.MaintenanceMode
{
	public class DeviceConfigMaintenanceModeResponse : BaseResponse<DeviceConfigMaintenanceModeDetails, AssetErrorInfo>
    {
        //Default constructor added for the deserialization
        public DeviceConfigMaintenanceModeResponse() { }

        public DeviceConfigMaintenanceModeResponse(IList<AssetErrorInfo> errors) : base(errors) { }

        public DeviceConfigMaintenanceModeResponse(AssetErrorInfo error) : base(error) { }

        public DeviceConfigMaintenanceModeResponse(IEnumerable<DeviceConfigMaintenanceModeDetails> movingThresholds, IList<AssetErrorInfo> errors = null) : base(movingThresholds, errors) { }

        [JsonProperty("deviceConfigMaintenanceModes", NullValueHandling = NullValueHandling.Ignore)]
        public override IEnumerable<DeviceConfigMaintenanceModeDetails> Lists { get; set; }
    }
}
