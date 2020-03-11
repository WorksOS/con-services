using System;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class AssetECMDataLinkDto
	{
		public Guid AssetECMInfoUID { get; set; }
		public long fk_ECMDataLinkID { get; set; }
		public Guid MachineComponentUID { get; set; }
	}
}
