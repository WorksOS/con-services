using CommonModel.Error;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientModel.DeviceConfig.Response.DeviceConfig.Ping
{
    public class DevicePingStatusResponse
    {
        public string DeviceUID { get; set; }
        public string AssetUID { get; set; }
        public string DevicePingLogUID { get; set; }
        public int RequestStatusID { get; set; }
        public string RequestState { get; set; }
        public DateTime RequestTimeUTC { get; set; }
        public DateTime RequestExpiryTimeUTC { get; set; }
        [JsonProperty("errors", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IList<IErrorInfo> Errors { get; set; }
    }
}
