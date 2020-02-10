using Newtonsoft.Json;

namespace CommonModel.DeviceSettings
{
    public class DeviceConfigurationDefaultsAndConfigurations
    {
        [JsonProperty("config", NullValueHandling = NullValueHandling.Ignore)]
        public DeviceConfigurationValues Configurations { get; set;}
        [JsonProperty("defaults", NullValueHandling = NullValueHandling.Ignore)]
        public DeviceConfigurationDefaults Defaults { get; set; }
    }
}