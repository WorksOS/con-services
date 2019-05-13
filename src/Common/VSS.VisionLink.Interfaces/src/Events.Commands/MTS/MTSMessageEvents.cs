using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;

namespace VSS.VisionLink.Interfaces.Events.Commands.MTS
{
	public class ConfigureSensorsEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public SensorDetail Sensor1 { get; set; }
		public SensorDetail Sensor2 { get; set; }
		public SensorDetail Sensor3 { get; set; }
	}

	public class ConfigureTpmsEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public bool Enabled { get; set; }
	}

	public class PollPositionEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
	}

	public class SetRuntimeMileageEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public double Mileage { get; set; }
		public long Runtime { get; set; }
	}

	public class SetStoppedThresholdEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public double Threshold { get; set; }
		public long Duration { get; set; }
		public bool Enabled { get; set; }
	}

	public class SetSpeedingThresholdEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public double Threshold { get; set; }
		public long Duration { get; set; }
		public bool Enabled { get; set; }
	}

	public class SetZoneLogicConfigEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public byte EntryHomeSiteSpeedMph { get; set; }
		public byte ExitHomeSiteSpeedMph { get; set; }
		public byte HysteresisHomeSiteSeconds { get; set; }
	}

	public class SetGeneralDeviceConfigEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public ushort DeviceShutdownDelaySeconds { get; set; }
		public ushort MdtShutdownDelaySeconds { get; set; }
		public bool AlwaysOnDevice { get; set; }
	}
	
	public class SetMovingConfigurationEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public ushort Radius { get; set; }
	}
	
	

	public class SetIgnitionReportingConfigurationEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public bool IgnitionReportingEnabled { get; set; }
	}

	

	public class SendPersonalityRequestEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
	}

	

	public class SendMachineSecuritySystemInformationMessageEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public MachineStartStatus? MachineStartStatus { get; set; } 
		public TamperResistanceStatus? TamperResistanceStatus { get; set; }
	}

	public class SendRadioMachineSecuritySystemInformationMessageEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public MachineStartStatus? MachineStartStatus { get; set; }
	}

	public class SetMainPowerLossReportingEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public bool PowerLossReportingEnabled { get; set; }
	}

	public class SetSuspiciousMoveEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public bool SuspiciousMoveEnabled { get; set; }
	}


	public class SetConfigureJ1939ReportingEvent : IMTSOutMessageEvent
	{
		
		public EventContext Context { get; set; }
		
		public bool ReportingEnabled { get; set; }
		public ReportType ReportType { get; set; }
		public List<J1939ParameterID> Parameters { get; set; }
		public bool IncludeSupportingParameters { get; set; }
	}

	

	public class SetMachineEventHeaderConfiguration : IMTSOutMessageEvent
	{

		public EventContext Context { get; set; }
		public PrimaryDataSourceEnum PrimaryDataSource { get; set; }

	}

	public class SetAssetBasedFirmwareVersion : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }

		public bool RFIDServicePlanAdded { get; set; }

		public AssetBasedFirmwareConfigurationType ConfigurationType { get; set; }
	}

	public class SendJ1939PublicParametersRequest : IMTSOutMessageEvent
	{
		public List<J1939ParameterID> Parameters { get; set; }

		public EventContext Context { get; set; }
	}

	/// <summary>
	/// Data is UTF8 encoded string of Byte array
	/// </summary>
	public class SendDeviceData : IMTSOutMessageEvent
	{
		public enum ControlTypeEnum
    {
      RawJ1939Data = 0x00,
      AsciiText = 0x01,
      UnicodeText = 0x02
    }

    public enum DestinationEnum
    {
      CanBusInstance1 = 0x00,
      CanBusInstance2 = 0x01
    }

		public EventContext Context { get; set; }

		public ControlTypeEnum ControlType { get; set; }
		public DestinationEnum Destination { get; set; }
		public string DataBase64 { get; set; }

	}
}
