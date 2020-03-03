using System;
using System.Collections.Generic;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;

namespace VSS.VisionLink.Interfaces.Events.Commands.A5N2
{
	public class AssetIdConfigurationChangedEvent : IOutMessageEvent
	{
		public Guid AssetUid { get; set; }		
		public string MakeCode { get; set; }
		public string SerialNumber { get; set; }
		public string VIN { get; set; }
		public string AssetAlias { get; set; }
		public EventContext Context { get; set; }
	}
	public class DisableMaintenanceModeEvent : IOutMessageEvent
	{
		public EventContext Context { get; set; }
	}

	public class DisableRapidReportingEvent : IOutMessageEvent
	{
		public EventContext Context { get; set; }
	}

	public class DigitalSwitchConfigurationEvent : IOutMessageEvent
	{
		public int SwitchNumber { get; set; }
		public string SwitchOnDescription { get; set; }
		public double Sensitivity { get; set; }
		public SwitchState DefaultState { get; set; }
		public DigitalInputMonitoringConditions MonitoredWhen { get; set; }
		public EventContext Context { get; set; }
	}

	public class DiscreteInputConfigurationEvent : IOutMessageEvent
	{
		public int SwitchNumber { get; set; }
		public string Name { get; set; }
		public string OpenDescription { get; set; }
		public string ClosedDescription { get; set; }
		public double Sensitivity { get; set; }
		public DigitalInputMonitoringConditions? MonitoredWhen { get; set; }
		public bool Enabled { get; set; }
		public bool WakeUpWhenClosed { get; set; }
		public EventContext Context { get; set; }
	}
	public class EnableMaintenanceModeEvent : IOutMessageEvent
	{
		public DateTime StartUtc { get; set; }
		public TimeSpan Duration { get; set; }
		public EventContext Context { get; set; }
	}

	public class EnableRapidReportingEvent : IOutMessageEvent
	{
		public EventContext Context { get; set; }
	}
	/// <summary>
	/// can also use MTS SendDailyReportConfigEvent
	/// </summary>
	public class FirstDailyReportStartTimeUtcChangedEvent : IOutMessageEvent
	{
		public DateTime DailyReportTimeUTC { get; set; }
		public EventContext Context { get; set; }
	}

	public class HourMeterModifiedEvent : IOutMessageEvent
	{
		public double? HoursBefore { get; set; }
		public double HoursAfter { get; set; }
		public EventContext Context { get; set; }
	}
	public class LocationStatusUpdateRequestedEvent : IOutMessageEvent
	{
		public EventContext Context { get; set; }
	}

	public class MovingCriteriaConfigurationChangedEvent : IOutMessageEvent
	{
		public double MovementRadiusInFeet { get; set; }
		public double MovementDurationSeconds { get; set; }
		public double MovementSpeedMPH { get; set; }
		public EventContext Context { get; set; }
	}

	public class OdometerModifiedEvent : IOutMessageEvent
	{
		public double? MilesBefore { get; set; }
		public double MilesAfter { get; set; }
		public EventContext Context { get; set; }
	}

	public class ReportingFrequencyChangedEvent : IOutMessageEvent
	{
		public int Frequency { get; set; }
		public int Interval { get; set; }
		public EventContext Context { get; set; }
	}

	public class SetDailyReportFrequencyEvent : IOutMessageEvent
	{
		public int Value { get; set; }
		public EventContext Context { get; set; }
	}

	public class SiteDispatchedEvent : IOutMessageEvent
	{
		public Guid SiteId { get; set; }
		public string Name { get; set; }
		public string PolygonWKT { get; set; }
		public EventContext Context { get; set; }
	}

	public class SiteRemovedEvent : IOutMessageEvent
	{
		public Guid SiteId { get; set; }
		public EventContext Context { get; set; }
	}

	public class GetStartModeEvent : IOutMessageEvent
	{
		public EventContext Context { get; set; }
	}

	public class SetStartModeEvent : IOutMessageEvent
	{
		public MachineStartStatus StartMode { get; set; }
		public EventContext Context { get; set; }
	}
	public class GetTamperLevelEvent : IOutMessageEvent
	{
		public EventContext Context { get; set; }
	}

	public class SetTamperLevelEvent : IOutMessageEvent
	{
		public TamperResistanceStatus TamperLevel { get; set; }
		public EventContext Context { get; set; }
	}

    public class HourMeterOffsetEvent : IOutMessageEvent
    {
        public Guid AssetId { get; set; }
        public double Offset { get; set; }
        public EventContext Context { get; set; }
    }

    public class OdometerOffsetEvent : IOutMessageEvent
    {
        public Guid AssetId { get; set; }
        public double Offset { get; set; }
        public EventContext Context { get; set; }
    }
}
