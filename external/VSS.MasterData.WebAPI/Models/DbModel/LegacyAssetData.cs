using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class LegacyAssetData
	{
		public long LegacyAssetID { get; set; }
		public string AssetUID { get; set; }
		public string MakeCode { get; set; }
		public string SerialNumber { get; set; }
		public string Model { get; set; }
		public string ModelYear { get; set; }
		public string AssetName { get; set; }
		public string DeviceType { get; set; }
		public string DeviceSerialNumber { get; set; }
		public string ProductFamily { get; set; }
		public string MakeName { get; set; }

		public string EquipmentVIN { get; set; }
	}
}