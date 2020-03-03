using System;
using System.Collections.Generic;
using VSS.MasterData.WebAPI.KafkaModel.Device;

namespace VSS.MasterData.WebAPI.ClientModel.Device
{
	/// <summary>
	/// Device properties payload details
	/// </summary>
	public class DevicePropertiesPayLoad
	{
		/// <summary>
		/// Device unique identifier
		/// </summary>
		public Guid DeviceUID { get; set; }
		/// <summary>
		/// Device serial number
		/// </summary>
		public string DeviceSerialNumber { get; set; }
		/// <summary>
		/// Device type
		/// </summary>
		public string DeviceType { get; set; }
		/// <summary>
		/// Device state
		/// </summary>
		public string DeviceState { get; set; }
		/// <summary>
		/// Device deregistered UTC time
		/// </summary>
		public DateTime? DeregisteredUTC { get; set; }
		/// <summary>
		/// Device module type
		/// </summary>
		public string ModuleType { get; set; }
		/// <summary>
		/// Device data link type
		/// </summary>
		public string DataLinkType { get; set; }
		/// <summary>
		/// Device personality details for payload
		/// </summary>
		public List<DevicePersonalityPayload> Personalities { get; set; }
	}
}