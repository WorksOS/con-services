using System;
using System.ComponentModel.DataAnnotations;
using VSS.MasterData.WebAPI.ClientModel;

namespace VSS.MasterData.WebAPI.ClientModel.Device
{
	/// <summary>
	/// Update device event details
	/// </summary>
	public class UpdateDeviceEvent : DeviceEventBase
	{
		/// <summary>
		/// Customer UID of the owner
		/// </summary>
		public Guid? OwningCustomerUID { get; set; }

		/// <summary>
		/// Device type MANUAL,PL,MTS etc.
		/// </summary>
		public string DeviceType { get; set; }

		/// <summary>
		/// Device state Provisioned, subscribed etc.
		/// </summary>
		public string DeviceState { get; set; }
	}
}