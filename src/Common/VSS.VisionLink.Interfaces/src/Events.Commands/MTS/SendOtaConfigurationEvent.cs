using System;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;

namespace VSS.VisionLink.Interfaces.Events.Commands.MTS
{
	public class SendOtaConfigurationEvent : IMTSOutMessageEvent
	{
		public OtaConfigDetail Input1 { get; set; }
		public OtaConfigDetail Input2 { get; set; }
		public OtaConfigDetail Input3 { get; set; }
		public OtaConfigDetail Input4 { get; set; }

		public TimeSpan? Smu { get; set; } 
		public bool? MaintenanceModeEnabled { get; set; }
		public TimeSpan? MaintenanceModeDuration { get; set; }
		public uint SequenceID { get; set; }
		public EventContext Context { get; set; }
	}
}