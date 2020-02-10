using ClientModel.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ClientModel.AssetSettings.Request
{
	public class AssetDeviceTypeRequest : IServiceRequest
    {
        [JsonProperty(PropertyName = "assetUID")]
        public List<Guid> AssetUIDs { get; set; }
        [JsonIgnore]
        public Guid? CustomerUid { get; set; }
        [JsonIgnore]
        public Guid? UserUid { get; set; }
        [JsonIgnore]
        public string UserGuid { get; set; }
        [JsonIgnore]
        public string CustomerGuid { get; set; }
        [JsonIgnore]
        public string SubAccountCustomerUid { get; set; }
        [JsonIgnore]
        public int StatusInd { get; set; }
        [JsonIgnore]
        public bool IsSwitchRequest { get; set; }
        [JsonIgnore]
        public string DeviceType { get; set; }
        [JsonProperty(PropertyName = "allAssets")]
        public bool AllAssets { get; set; }
    }
}
