using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClientModel.DeviceConfig.Request
{ 
    public class DevicePingLogRequest: IServiceRequest
    {
        public Guid AssetUID {get; set;}
        public Guid DeviceUID { get; set; }
        [JsonIgnore]
        public string FamilyName { get; set; }
        [JsonIgnore]
        public Guid? CustomerUID { get; set; }
        [JsonIgnore]
        public Guid? UserUID { get; set; }
        [JsonIgnore]
        public Guid DevicePingLogUID { get; set; }
    }
}