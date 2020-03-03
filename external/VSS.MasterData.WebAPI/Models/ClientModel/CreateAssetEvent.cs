using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class CreateAssetEvent
	{
		/// <summary>
		/// 
		/// </summary>
		public string AssetName { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public long LegacyAssetID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Required]
		public string SerialNumber { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Required]
		public string MakeCode { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Model { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string AssetType { get; set; } // Product Family

		/// <summary>
		/// 
		/// </summary>
		public int? IconKey { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string EquipmentVIN { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int? ModelYear { get; set; }

		/// <summary>
		/// 
		/// </summary>		 
		public Guid? AssetUID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Guid? OwningCustomerUID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime? ActionUTC { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime? ReceivedUTC { get; set; }

		public string ObjectType { get; set; }

		public string Category { get; set; }

		// public string Project { get; set; }

		public string ProjectStatus { get; set; }

		public string SortField { get; set; }

		public string Source { get; set; }

		public string UserEnteredRuntimeHours { get; set; }

		public string Classification { get; set; }

		public string PlanningGroup { get; set; }
 	}
}