using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;

namespace VSS.VisionLink.Interfaces.Events.Commands.PL
{
	public class SendQueryCommandEvent : IPLOutMessageEvent
	{
		public EventContext Context { get; set; }
		public PLQueryCommandEnum Command { get; set; }
	}

	public class SendGeoFenceConfig : IPLOutMessageEvent
	{
		public EventContext Context { get; set; }
		public bool InclusiveProductWatch { get; set; }
		public decimal InclusiveLatitude { get; set; }
		public decimal InclusiveLongitude { get; set; }
		public decimal InclusiveRadiusKilometers { get; set; }
    public decimal[] ExclusiveLatitude { get; set; }
		public decimal[] ExclusiveLongitude { get; set; }
	  public decimal[] ExclusiveRadius { get; set; }

	}

	public class SendReportIntervalsConfig : IPLOutMessageEvent
	{
		public EventContext Context { get; set; }

		public TimeSpan? EventIntervals  { get; set; }
		public EventFrequency? Level1TransmissionFrequency  { get; set; }
		public EventFrequency? Level2TransmissionFrequency  { get; set; }
		public EventFrequency? Level3TransmissionFrequency  { get; set; }
    public TimeSpan? NextMessageInterval  { get; set; }
		public bool? GlobalGramEnable  { get; set; }
		public DateTime? ReportStartTimeUTC  { get; set; }
		public EventFrequency? DiagnosticTransmissionFrequency  { get; set; }
    public SMUFuelReporting? SmuFuelReporting  { get; set; }
		public bool? StartStopConfigEnabled  { get; set; }
		public int? PositionReportConfig { get; set; }
	}

	public class SendRuntimeAdjustmentConfig : IPLOutMessageEvent
	{
		public EventContext Context { get; set; }
		public TimeSpan NewRuntimeValue { get; set; }
	}

	public class Send24by7TimeFenceConfig : IPLOutMessageEvent
	{
		public EventContext Context { get; set; }
	}

	public class SendProductWatchActivation : IPLOutMessageEvent
	{
		public EventContext Context { get; set; }
		public bool? InclusiveWatchActive { get; set; }
		public bool? ExclusiveWatchActive { get; set; }
		public bool? TimeBasedWatchActive { get; set; }
	}

	

	public class SendDigitalInputConfig : IPLOutMessageEvent
	{
		public EventContext Context { get; set; }

		public DigitalInputConfigDetails Config1 { get; set; }
		public DigitalInputConfigDetails Config2 { get; set; }
		public DigitalInputConfigDetails Config3 { get; set; }
		public DigitalInputConfigDetails Config4 { get; set; }

	}

}
