using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.MasterData.WebAPI.KafkaModel.Device
{
	public enum DevicePersonalityTypeEnum
	{
		DevicePartNumber = 0,
		FirmwarePartNumber = 1,
		MainboardSoftwareVersion = 2,
		GatewayFirmwarePartNumber = 3,
		CellModemIMEI = 4,
		NetworkFirmwarePartnumber = 5,
		CellularFirmwarePartnumber = 6,
		SatelliteFirmwarePartnumber = 7,
		RadioFirmwarePartNumber = 8
	}
}
