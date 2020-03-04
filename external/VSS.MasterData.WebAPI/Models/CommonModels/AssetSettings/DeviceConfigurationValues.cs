using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.AssetSettings
{
	public class DeviceConfigurationValues
	{
		[JsonProperty("switches", NullValueHandling = NullValueHandling.Ignore)]
		public DeviceConfigurationSwitchesConfig SwitchesConfig { get; set; }

		[JsonProperty("meters", NullValueHandling = NullValueHandling.Ignore)]
		public DeviceConfigurationMetersConfig MetersConfig { get; set; }
		[JsonProperty("settings", NullValueHandling = NullValueHandling.Ignore)]
		public DeviceConfigurationSettingsConfig SettingsConfig { get; set; }
	}
}
