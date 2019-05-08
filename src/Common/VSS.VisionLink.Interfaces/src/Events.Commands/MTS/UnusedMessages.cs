using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Interfaces.Events.Commands.MTS
{
	/*public class SendMachineEventConfigEvent : IMTSOutMessageEvent
	{
		public class MachineEventConfigBlock
		{
			public enum MachineEventDeliveryMode
			{
				Suppress = 0,
				Immediate = 1,
				DailyReport = 2,
			}
			public MachineEventDeliveryMode DeliveryMode;
			public enum TriggerType
			{
				None,
				Ignition,
				DiscreteInput,
				Speeding,
				Moving,
				Site,
				Engine,
				MSSKeyID,
				Daily
			}
			public TriggerType Trigger;
			public enum TriggerResponseType
			{
				PositionReport,
				FuelReport,
				ECMInfo,
				GatewayAdmin,
				MaintenanceAdmin,
			}
			public List<TriggerResponseType> Responses;
		}

		public List<MachineEventConfigBlock> ConfigBlocks { get; set; }
		public EventContext Context { get; set; }
	}*/

	/*public class SendPort1033SkylineFirmWareUpdateCommandEvent : IMTSOutMessageEvent
	{
		public string GpsDeviceID { get; set; }
		public byte FwRequest { get; set; }
		public byte? Target { get; set; } 
		public bool? ForceDirectory { get; set; } 
		public bool? VersionNumbersIncluded { get; set; } 
		public string FtpHostName { get; set; }
		public string FtpUserName { get; set; } 
		public string FtpPassword { get; set; } 
		public string SourcePath { get; set; } 
		public string DestinationPath { get; set; } 
		public byte? FwMajor { get; set; } 
		public byte? FwMinor { get; set; } 
		public byte? FwBuildType { get; set; } 
		public byte? HwMajor { get; set; }
		public byte? HwMinor { get; set; }

		public EventContext Context { get; set; }
	}*/

	/*public class SetDriverIDConfigEvent : IMTSOutMessageEvent
	{
		public enum DriverIDCharSet
		{
			AlphaOnly = 0x00,
			NumericOnly = 0x01,
			AlphaNumeric = 0x02
		}

		public EventContext Context { get; set; }
		public bool DriverIDEnabled { get; set; }
		public bool EnableMdtDriverEntry { get; set; }
		public bool ForceEntryAndLogOut { get; set; }
		public DriverIDCharSet CharSet { get; set; }
		public byte MdtIDMax { get; set; }
		public byte MdtIDMin { get; set; }
		public byte DisplayedListSize { get; set; }
		public byte StoredListSize { get; set; }
		public bool ForcedLogon { get; set; }
		public bool AutoLogoutInvalid { get; set; }
		public TimeSpan AutoLogoutTime { get; set; }
		public bool ExpireMru { get; set; }
		public DateTime MruExpiry { get; set; }
		public bool ExpireUnvalidatedMRUs { get; set; }
		public TimeSpan UnvalidatedExpiry { get; set; }
		public bool DisplayMechanic { get; set; }
		public string MechanicId { get; set; }
		public string MechanicDisplayName { get; set; }
		public bool EnableLoggedIn { get; set; }
		public byte LoggedInoutputPolarity { get; set; }
		

	}*/
	/*
	/// <summary>
	/// Either provide Apn or other info
	/// </summary>
	public class SetNetworkInterfaceConfigurationEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public string NewApn { get; set; }
		//if Apn is given, below are optional
		public string StackConfig1 { get; set; }
		public string StackConfig2 { get; set; }
		public string StackConfig3 { get; set; }
		public string StackConfig4 { get; set; }
		public string AppConfig { get; set; }
	}*/
	/*
	public class QueryBITReportEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public QueryCommandType WhichReport { get; set; }
	}*/
	/*public class SendDeviceConfigurationQueryCommandEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public QueryCommandType Command { get; set; }
	}

	public class SendFirmwareRequestMessageEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public long MtsFirmwareVersionID { get; set; }
		public string Host { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
	}*/

	/*public class CancelFirmwareRequestMessageEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public DateTime? DueUtc { get; set; }
	}*/

	/*public class SetRadioTransmitterDisableControl : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public bool Enabled { get; set; }
	}*/

	/*public class SendPasscodeEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public string Passcode { get; set; }
	}*/
	/*public class SetDevicePortConfigEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public string PortNumber { get; set; }
		public string ServiceType { get; set; }
	}*/

	/*public class SetPrimaryIPAddressConfigurationEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public string IPAddress { get; set; }
		public bool IsTcp { get; set; }
		public short? OtherPort { get; set; }
	}*/

	/*public class CalibrateDeviceRuntimeEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public double NewRuntimeHours { get; set; }
	}*/


}
