using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.AssetSettings
{
	public class DeviceConfigurationDefaults
	{
		[JsonProperty(PropertyName ="movingThreshold", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public MovingThreshold MovingThreshold { get; set; }
	}

	public class MovingThreshold
	{
		[JsonProperty(PropertyName = "movingOrStoppedThreshold", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public double MovingOrStoppedThreshold { get; set; }
		[JsonProperty(PropertyName = "movingThresholdsDuration", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int MovingThresholdsDuration { get; set; }
		[JsonProperty(PropertyName = "movingThresholdsRadius", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int MovingThresholdsRadius { get; set; }
	}
}
