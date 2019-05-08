using System;

namespace VSS.VisionLink.Interfaces.Events.Commands.Models
{
	public class OtaConfigDetail
	{
		public InputConfigType InputConfig { get; set; }
		public TimeSpan? InputDelay { get; set; }
		public string InputDesc { get; set; }
		public DigitalInputMonitoringConditions? MonitoringCondition { get; set; }
	}
}