using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.MasterData.WebAPI.ClientModel
{
    public class Asset
    {
		public string AssetUID { get; set; }
		public string AssetName { get; set; }
        public long LegacyAssetID { get; set; }
        public string SerialNumber { get; set; }
        public string MakeCode { get; set; }
        public string Model { get; set; }
        public string AssetType { get; set; }
        public int? IconKey { get; set; }
        public string EquipmentVIN { get; set; }
        public int? ModelYear { get; set; }
		public int StatusInd { get; set; }
		public string OwningCustomerUID { get; set; }
		public string ObjectType { get; set; }
		public string Category { get; set; }
		public string ProjectStatus { get; set; }
		public string SortField { get; set; }
		public string Source { get; set; }
		public string UserEnteredRuntimeHours { get; set; }
		public string Classification { get; set; }
		public string PlanningGroup { get; set; }
		public DateTime InsertUTC { get; set; }
		public DateTime UpdateUTC { get; set; }
	}
}