using System;

namespace VSS.MasterData.WebAPI.ClientModel
{
	public class DeviceModel
	{
		/// <summary>
		/// 
		/// </summary>
		public string DeviceSerialNumber { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string DeviceType { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string DeviceState { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Guid? DeviceUID { get; set; }
	}
}