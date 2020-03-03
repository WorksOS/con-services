using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class DeleteAssetEvent
	{
		/// <summary>
		/// 
		/// </summary>
		[Required]
		public Guid AssetUID { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[Required]
		public DateTime ActionUTC { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime ReceivedUTC { get; set; }
	}
}