using System;

namespace VSS.MasterData.WebAPI.ClientModel.Device
{
	public class DevicePropertiesV2
	{
		public Guid DeviceUID { get; set; }
		public string DeviceSerialNumber { get; set; }
		public string DeviceType { get; set; }
		public int DeviceState { get; set; }
		public DateTime? DeregisteredUTC { get; set; }
		public string ModuleType { get; set; }
		public string DataLinkType { get; set; }
		public int PersonalityTypeId { get; set; }
		public string PersonalityDescription { get; set; }
		public string PersonalityValue { get; set; }
		public DateTime ActionUTC { get; set; }
		public DateTime ReceivedUTC { get; set; }
	}

	public class AssetDevicePropertiesV2 : DevicePropertiesV2
	{
		public Guid AssetUID { get; set; }
	}
}
