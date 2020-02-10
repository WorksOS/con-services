using ClientModel.Response;
using CommonModel.Error;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ClientModel.AssetSettings.Response
{
	public class DeviceTypeListResponse : BaseResponse<DeviceType, ErrorInfo>
    {
        public DeviceTypeListResponse(IList<DeviceType> lists, IList<ErrorInfo> errors) : base(lists) { this.Errors = errors; }

        public DeviceTypeListResponse(IList<ErrorInfo> errors): base(errors) { }

        public DeviceTypeListResponse(ErrorInfo error) : base(error) { }

        public DeviceTypeListResponse(IList<DeviceType> lists) : base(lists) { }

        [JsonProperty("deviceTypes", NullValueHandling = NullValueHandling.Ignore)]
        public override IList<DeviceType> Lists { get; set; }
    }
}
