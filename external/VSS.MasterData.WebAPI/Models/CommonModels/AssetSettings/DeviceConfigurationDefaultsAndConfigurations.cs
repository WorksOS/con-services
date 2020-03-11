using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.AssetSettings
{
	public class DeviceConfigurationDefaultsAndConfigurations
	{
		[JsonProperty("config", NullValueHandling = NullValueHandling.Ignore)]
		public DeviceConfigurationValues Configurations { get; set; }
		[JsonProperty("defaults", NullValueHandling = NullValueHandling.Ignore)]
		public DeviceConfigurationDefaults Defaults { get; set; }
	}
}
