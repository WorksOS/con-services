using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Interfaces.Events.Commands.Models
{
	public enum RFIDReaderType : byte
	{
		TMVegaM5e = 0,
		TMM6e = 1,
		TMVegaM5eEU = 2
	}

	public enum RFIDReaderStatusType : byte
	{
		DisableRFIDReader = 0,
		EnableRFIDReader = 1
	}


	public enum RFIDTriggerSourceType : byte
	{
		EnabledByIgnition = 0,
		EnabledByEventsOrATCommands = 1,
		EnabledBySiteLogic = 2,
		DigitalOutputFromTelematicsDeviceOnly = 3,
		DigitalInputFromExternalAccessory = 4
	}
	public enum AntennaSwitchingMethodType : byte
	{
		Dynamic = 0,
		EqualTime = 1
	}
	public enum LinkRateType : short
	{
		KHz250 = 250,
		KHz640 = 640
	}

	public enum TariType : byte
	{
		Us25 = 0,
		Us12_5 = 1,
		Us6_25 = 2
	}

	public enum MillerValueType : byte
	{
		FM0 = 0,
		M2 = 1,
		M4 = 2,
		M8 = 3
	}

	public enum SessionForRfidConfigurationType : byte
	{
		S0 = 0,
		S1 = 1,
		S2 = 2,
		S3 = 3
	}

	public enum TargetForRfidConfigurationType : byte
	{
		A = 0,
		B = 1,
		AB = 2,
		BA = 3
	}

	public enum BaudRateForRfidConfigurationType : byte
	{
		BaudRate9600bps = 1,
		BaudRate19200bps = 2,
		BaudRate38400bps = 3,
		BaudRate57600bps = 4,
		BaudRate115200bps = 5,
		BaudRate230400bps = 6
	}
	public enum ReaderOperationRegionForRfidConfigurationType : byte
	{
		NA = 0,
		EU = 1,
		AU = 2,
		KR = 3,
		IN = 4
	}
}
