using System;
using System.ComponentModel.DataAnnotations;
using VSS.MasterData.WebAPI.ClientModel;

namespace VSS.MasterData.WebAPI.ClientModel.Device
{
	/// <summary>
	/// Create device event details
	/// </summary>
	public class CreateDeviceEvent: DeviceEventBase
	{
		/// <summary>
		/// Device type MANUAL,PL,MTS etc.
		/// </summary>
		[Required]
		public string DeviceType { get; set; }

		/// <summary>
		/// Device state Provisioned, subscribed etc.
		/// </summary>
		[Required]
		public string DeviceState { get; set; }
	}
}
