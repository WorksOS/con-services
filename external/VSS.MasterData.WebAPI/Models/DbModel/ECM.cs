using System;

namespace VSS.MasterData.WebAPI.DbModel
{

	//[TableName("msg_md_assetecm_AssetEcmInfo")]
	public class ECM
	{
		public Guid AssetUID { get; set; }
		public string ECMSerialNumber { get; set; }
		public string FirmwarePartNumber { get; set; }
		public string ECMDescription { get; set; }
		//[ColumnName("IsSyncClockEnabled")]
		public bool SyncClockEnabled { get; set; }
		public bool SyncClockLevel { get; set; }
		public DateTime EventUTC { get; set; }
		public Guid AssetECMInfoUID { get; set; }
		public Guid DeviceUID { get; set; }
	}
}
