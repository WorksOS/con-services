using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ClientModel.DeviceConfig.Request.DeviceConfig
{
    public class DeviceConfigRequestBase : IServiceRequest
    {
        private bool? _isAssetUIDValidationRequired;

        [JsonProperty("assetUIDs", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> AssetUIDs { get; set; }
        [JsonProperty("deviceType", NullValueHandling = NullValueHandling.Ignore)]
        public string DeviceType { get; set; }
        [JsonIgnore]
        public string ParameterGroupName { get; set; }
        [JsonIgnore]
        public Guid? CustomerUID { get; set; }
        [JsonIgnore]
        public Guid? UserUID { get; set; }
        [JsonIgnore]
        public Dictionary<string, string> ConfigValues { get; set; }
        [JsonIgnore]
        public int SwitchNumber { get; set; }
        [JsonIgnore]
        public bool IsAssetUIDValidationRequired
        {
            get
            {
                if (!_isAssetUIDValidationRequired.HasValue)
                {
                    _isAssetUIDValidationRequired = true;
                }
                return _isAssetUIDValidationRequired.Value;
            }
            set
            {
                _isAssetUIDValidationRequired = value;
            }
        }
    }
}
