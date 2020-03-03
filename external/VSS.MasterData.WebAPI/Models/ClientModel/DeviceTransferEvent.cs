using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.ClientModel.Device
{
	/// <summary>
	/// Device transfer event details
	/// </summary>
	public class DeviceTransferEvent
	{
		/// <summary>
		/// Device unique identifier
		/// </summary>
		[Required]
		public Guid DeviceUID { get; set; }
		/// <summary>
		/// Old Asset unique identifier
		/// </summary>
		[Required]
		public Guid OldAssetUID { get; set; }
		/// <summary>
		/// New Asset unique identifier
		/// </summary>
		[Required]
		public Guid NewAssetUID { get; set; }
		/// <summary>
		/// Device transfer event action UTC time
		/// </summary>
		[Required]
		public DateTime? ActionUTC { get; set; }
		/// <summary>
		/// Device transfer event received UTC time
		/// </summary>
		public DateTime ReceivedUTC { get; set; }
	}
}
