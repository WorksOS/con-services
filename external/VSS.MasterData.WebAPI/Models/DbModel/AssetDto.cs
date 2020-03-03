using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.MasterData.WebAPI.DbModel.Device
{
	public class AssetDto 
	{
		public string AssetName { get; set; }
		public long? LegacyAssetID { get; set; }
		public string Model { get; set; }
		public string AssetType { get; set; } //Product Family
		public int? IconKey { get; set; }
		public string EquipmentVIN { get; set; }
		public int? ModelYear { get; set; }
		public Guid OwningCustomerUID { get; set; }
		public Guid AssetUID { get; set; }

	}
}
