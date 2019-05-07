using System;
using VSS.VisionLink.Interfaces.Events.Commands.Interfaces;
using VSS.VisionLink.Interfaces.Events.Commands.Models;

namespace VSS.VisionLink.Interfaces.Events.Commands.MTS
{
	public class SendRfidConfigurationEvent : IMTSOutMessageEvent
	{
		public EventContext Context { get; set; }
		public RFIDReaderType RfidReaderType { get; set; }
		public RFIDReaderStatusType RfidReaderStatus { get; set; }
		public RFIDTriggerSourceType RfidTriggerSourceType { get; set; }
		public UInt16 TxRFPower { get; set; }
		public UInt16 AsynOnTime { get; set; }
		public UInt16 AsynOffTime { get; set; }
		public AntennaSwitchingMethodType AntennaSwitchingMethod { get; set; }
    public LinkRateType LinkRate { get; set; }
    public TariType Tari { get; set; }
    public MillerValueType MillerValue { get; set; }
    public SessionForRfidConfigurationType Session { get; set; }
    public TargetForRfidConfigurationType Target { get; set; }
    public bool Gen2QHasFixedQValue { get; set; }
		public byte Gen2QFixedQValue { get; set; }
		public BaudRateForRfidConfigurationType BaudRate { get; set; }
		public ReaderOperationRegionForRfidConfigurationType ReaderOperationRegion { get; set; }
	}
}