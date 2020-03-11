using System;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class AssetDetail
	{
		public string AssetUID { get; set; }
		public string AssetName { get; set; }
		public string SerialNumber { get; set; }
		public string MakeCode { get; set; }
		public string Model { get; set; }
		public string AssetTypeName { get; set; }
		public int ModelYear { get; set; }
		public string OwningCustomerUID { get; set; }
		public string DeviceSerialNumber { get; set; }
		public string DeviceType { get; set; }
		public string DeviceUID { get; set; }
		public string DeviceState { get; set; }
		public DateTime TimestampOfModification { get; set; }
		public string AssetCustomerUIDs { get; set; }

	}
}