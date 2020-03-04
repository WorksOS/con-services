using System;

namespace VSS.VisionLink.Interfaces.Events.Commands.Models
{
	public class DigitalInputConfigDetails
	{
		public InputConfig? InputConfig { get; set; }
		public TimeSpan? InputDelayTime { get; set; }
		public DigitalInputMonitoringConditions? DigitalInputMonitoringCondition { get; set; }
		public string Description { get; set; }
	}
}