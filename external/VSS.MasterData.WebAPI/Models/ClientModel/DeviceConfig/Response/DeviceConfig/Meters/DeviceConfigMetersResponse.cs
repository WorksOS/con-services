using CommonModel.Error;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.Meters
{
    public class DeviceConfigMetersResponse : BaseResponse<DeviceConfigMetersDetails, AssetErrorInfo>
    {
        //Default constructor added for the deserialization
        public DeviceConfigMetersResponse() { }

        public DeviceConfigMetersResponse(IList<AssetErrorInfo> errors) : base(errors) { }

        public DeviceConfigMetersResponse(AssetErrorInfo error) : base(error) { }

        public DeviceConfigMetersResponse(IEnumerable<DeviceConfigMetersDetails> meters, IList<AssetErrorInfo> errors = null) : base(meters, errors) { }

        [JsonProperty("deviceConfigMeters", NullValueHandling = NullValueHandling.Ignore)]
        public override IEnumerable<DeviceConfigMetersDetails> Lists { get; set; }
    }
}
