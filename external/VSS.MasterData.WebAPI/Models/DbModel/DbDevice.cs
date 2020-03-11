using System;
using System.Collections.Generic;
using System.Text;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.DbModel.Device
{
	public class DbDevice:IDbTable
	{
		public int DeviceID { get; set; }
		public Guid DeviceUID { get; set; }
		public string SerialNumber { get; set; }
		public DateTime? DeregisteredUTC { get; set; }
		public string ModuleType { get; set; }
		public string MainboardSoftwareVersion { get; set; }
		public string FirmwarePartNumber { get; set; }
		public string GatewayFirmwarePartNumber { get; set; }
		public string DataLinkType { get; set; }
		public int fk_DeviceStatusID { get; set; }
		public DateTime InsertUTC { get; set; }
		public DateTime UpdateUTC { get; set; }
		public int fk_DeviceTypeID { get; set; }
		public string CellModemIMEI { get; set; }
		public string DevicePartNumber { get; set; }
		public string CellularFirmwarePartnumber { get; set; }
		public string NetworkFirmwarePartnumber { get; set; }
		public string SatelliteFirmwarePartnumber { get; set; }

		public string GetIgnoreColumnsOnUpdate()
		{
			return "DeviceID,DeviceUID,InsertUTC";//SerialNumber,fk_DeviceTypeID
		}

		public string GetTableName()
		{
			return "md_device_Device";
		}
		

		public string GetIdColumn()
		{
			return null;
		}
	}
}