using Newtonsoft.Json;

namespace CommonModel.DeviceSettings
{
    public class DeviceConfigurationValues
    {
        [JsonProperty("switches", NullValueHandling =NullValueHandling.Ignore)]
        public DeviceConfigurationSwitchesConfig SwitchesConfig { get; set; }

        [JsonProperty("meters", NullValueHandling = NullValueHandling.Ignore)]
        public DeviceConfigurationMetersConfig MetersConfig { get; set; }
        [JsonProperty("settings", NullValueHandling = NullValueHandling.Ignore)]
        public DeviceConfigurationSettingsConfig SettingsConfig { get; set; }
    }
}
