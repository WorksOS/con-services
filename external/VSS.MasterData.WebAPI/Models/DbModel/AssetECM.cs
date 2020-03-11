using static VSS.MasterData.WebAPI.Utilities.Enums.Enums;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class AssetECM
	{
		public string SerialNumber { get; set; }
		public string PartNumber { get; set; }
		public string SoftwarePartNumber { get; set; }
		public string Description { get; set; }
		public bool SyncClockEnabled { get; set; }
		public bool SyncClockLevel { get; set; }
		public string MID { get; set; }
		public string J1939Name { get; set; }
		public string SourceAddress { get; set; }
		public DataLinkEnum DataLink { get; set; }
		public string AssetECMInfoUID { get; set; }
	}
}
