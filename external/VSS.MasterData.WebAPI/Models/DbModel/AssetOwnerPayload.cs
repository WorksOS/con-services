using System;
using static VSS.MasterData.WebAPI.Utilities.Enums.Enums;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class AssetOwnerPayload
	{
		public Guid AssetUID { get; set; }
		public AssetOwnerInfo AssetOwnerRecord { get; set; }
		public Operation Action { get; set; }
		public DateTime ActionUTC { get; set; }
		public DateTime ReceivedUTC { get; set; }
	}
}
