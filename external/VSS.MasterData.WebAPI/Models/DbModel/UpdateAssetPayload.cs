using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class UpdateAssetPayload
	{
		public string AssetName { get; set; }

		public long? LegacyAssetID { get; set; }

		public string Model { get; set; }

		public string AssetType { get; set; }
	
		public string SerialNumber { get; set; }

		public string MakeCode { get; set; }

		public int? IconKey { get; set; }

		public string EquipmentVIN { get; set; }

		public int? ModelYear { get; set; }

		public Guid? OwningCustomerUID { get; set; }

		[Required]
		public Guid AssetUID { get; set; }
		
		public string ObjectType { get; set; }

		public string Category { get; set; }

		// public string Project { get; set; }

		public string ProjectStatus { get; set; }

		public string SortField { get; set; }

		public string Source { get; set; }

		public string UserEnteredRuntimeHours { get; set; }

		public string Classification { get; set; }

		public string PlanningGroup { get; set; }

		public DateTime ActionUTC { get; set; }

		public DateTime ReceivedUTC { get; set; }
	}
}