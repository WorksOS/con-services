using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.AssetSettings
{
	public class DeviceConfigurationSwitchesConfig
	{
		// {"physicalSwitchNumber":1,"switchLabel":"","maskedSwitchNumber":1,"isSingleState":0}
		[JsonProperty("physicalSwitchNumber", NullValueHandling = NullValueHandling.Ignore)]
		public int SwitchNumber { get; set; }
		[JsonProperty("switchLabel", NullValueHandling = NullValueHandling.Ignore)]
		public string SwitchLabel { get; set; }
		[JsonProperty("maskedSwitchNumber", NullValueHandling = NullValueHandling.Ignore)]
		public int MaskedSwitchNumber { get; set; }
		[JsonProperty("isSingleState", NullValueHandling = NullValueHandling.Ignore)]
		public bool isSingleState { get; set; }
		//{"config":{"allowBackward":true},"defaults":{}}

		[JsonProperty("isTampered", NullValueHandling = NullValueHandling.Ignore)]
		public bool isTampered { get; set; }
		[JsonProperty("switchPowerType", NullValueHandling = NullValueHandling.Ignore)]
		public string SwitchPowerType { get; set; }
	}
}
