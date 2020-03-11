using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.AssetSettings
{
	public class DeviceConfigurationMetersConfig
	{
		[JsonProperty("allowBackward", NullValueHandling = NullValueHandling.Ignore)]
		public bool AllowBackward { get; set; }

		[JsonProperty("notAllowedModuleType", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> NotAllowedModuleType { get; set; }
	}
}
