using System;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.ClientModel.Device
{
	/// <summary>
	/// Device Replacement Event details
	/// </summary>
	public class DeviceReplacementEvent
	{
		/// <summary>
		/// Old device unique identifier
		/// </summary>
		[Required]
		public Guid OldDeviceUID { get; set; }
		/// <summary>
		/// New device unique identifier
		/// </summary>
		[Required]
		public Guid NewDeviceUID { get; set; }
		/// <summary>
		/// Asset unique identifier
		/// </summary>
		[Required]
		public Guid AssetUID { get; set; }
		/// <summary>
		/// Device replacement event action UTC time
		/// </summary>
		[Required]
		public DateTime? ActionUTC { get; set; }
		/// <summary>
		/// Device replacement event received UTC time
		/// </summary>
		public DateTime ReceivedUTC { get; set; }
	}
}
