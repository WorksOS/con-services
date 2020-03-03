using System;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class AssetDeviceForSupportUserD
	{
		public Guid? AssetUID { get; set; }
		public string AssetName { get; set; }
		public string AssetSerialNumber { get; set; }
		public string AssetMakeCode { get; set; }
		public string DeviceSerialNumber { get; set; }
		public string DeviceType { get; set; }
		public Guid? DeviceUID { get; set; }
	}
}
