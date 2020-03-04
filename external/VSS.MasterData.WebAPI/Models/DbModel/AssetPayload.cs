using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VSS.MasterData.WebAPI.Data.MySql;

namespace VSS.MasterData.WebAPI.DbModel
{
	public class AssetPayload : IDbTable
	{
		public Guid AssetUID { get; set; }

		public Guid? OwningCustomerUID { get; set; }

		public string AssetName { get; set; }

		public long LegacyAssetID { get; set; }

		public string SerialNumber { get; set; }
				
		public string MakeCode { get; set; }

		public string Model { get; set; }

		public string AssetTypeName { get; set; }

		public int? IconKey { get; set; }

		public string EquipmentVIN { get; set; }

		public int? ModelYear { get; set; }
				
		public bool StatusInd { get; set; }
				
		public DateTime InsertUTC { get; set; }

		public DateTime UpdateUTC { get; set; }

		public string ObjectType { get; set; }

		public string Category { get; set; }

		// public string Project { get; set; }

		public string ProjectStatus { get; set; }

		public string SortField { get; set; }

		public string Source { get; set; }

		public string UserEnteredRuntimeHours { get; set; }

		public string Classification { get; set; }

		public string PlanningGroup { get; set; }

		public string GetIgnoreColumnsOnUpdate()
		{
			return "AssetID,SerialNumber,MakeCode,InsertUTC,StatusInd";
		}

		public string GetTableName()
		{
			return "md_asset_Asset";
		}

		public string GetIdColumn()
		{
			return null;
		}
	}
}
