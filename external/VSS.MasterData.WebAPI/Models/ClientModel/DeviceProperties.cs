using System;

namespace VSS.MasterData.WebAPI.ClientModel.Device
{
	public class DeviceProperties 
	{
		public string MainboardSoftwareVersion { get; set; }
		public string RadioFirmwarePartNumber { get; set; }
		public string GatewayFirmwarePartNumber { get; set; }
		public string FirmwarePartNumber { get; set; }
		public string DevicePartNumber { get; set; }
		public string CellularFirmwarePartnumber { get; set; }
		public string SatelliteFirmwarePartnumber { get; set; }
		public string CellModemIMEI { get; set; }
		public string Description { get; set; }
		public string NetworkFirmwarePartnumber { get; set; }
		public string DeviceUID { get; set; }
		public string DeviceSerialNumber { get; set; }
		public string DeviceType { get; set; }
		public string DeviceState { get; set; }
		public DateTime? DeregisteredUTC { get; set; }
		public string ModuleType { get; set; }
		public string DataLinkType { get; set; }
	}
}
