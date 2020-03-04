using System;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class AssetDeviceDto
	{
		public string AssetID { get; set; }
		public string DeviceID { get; set; }
		public Guid AssetUID => !string.IsNullOrEmpty(AssetID) ? Guid.Parse(AssetID) : Guid.Empty;
		public Guid DeviceUID => !string.IsNullOrEmpty(DeviceID) ? Guid.Parse(DeviceID) : Guid.Empty;

		public string DeviceType { get; set; }

		public string ModuleType { get; set; }

	}
}
