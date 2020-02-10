using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.ClientModel.Device
{
	/// <summary>
	/// Update device properties
	/// </summary>
	public class UpdateDeviceProperties : IValidatableObject
	{

		/// <summary>
		/// Device serial number
		/// </summary>
		[Required]
		[StringLength(128)]
		public string DeviceSerialNumber { get; set; }
		/// <summary>
		/// Device type
		/// </summary>
		[Required]
		public string DeviceType { get; set; }
		/// <summary>
		/// Device module type
		/// </summary>
		[StringLength(50)]
		public string ModuleType { get; set; }
		/// <summary>
		/// Device mainboard software version
		/// </summary>
		[StringLength(50)]
		public string MainboardSoftwareVersion { get; set; }
		/// <summary>
		/// Device radio firmware part number
		/// </summary>
		[StringLength(50)]
		public string RadioFirmwarePartNumber { get; set; }
		/// <summary>
		/// Device gateway firmware part number
		/// </summary>
		[StringLength(50)]
		public string GatewayFirmwarePartNumber { get; set; }
		/// <summary>
		/// Device data link type
		/// </summary>
		[StringLength(50)]
		public string DataLinkType { get; set; }
		/// <summary>
		/// Device cellular firmware part number
		/// </summary>
		[StringLength(50)]
		public string CellularFirmwarePartnumber { get; set; }
		/// <summary>
		/// Device network firmware part number
		/// </summary>
		[StringLength(50)]
		public string NetworkFirmwarePartnumber { get; set; }
		/// <summary>
		/// Device satellite firmware part number
		/// </summary>
		[StringLength(50)]
		public string SatelliteFirmwarePartnumber { get; set; }
		/// <summary>
		/// Device update properties action UTC time
		/// </summary>
		public DateTime? ActionUTC { get; set; }
		/// <summary>
		/// Device update properties received UTC time
		/// </summary>
		public DateTime ReceivedUTC { get; set; }

		/// <summary>
		/// Device Update property Description
		/// </summary>
		public string Description { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (string.IsNullOrEmpty(DeviceSerialNumber))
				yield return new ValidationResult("The DeviceSerialNumber field is required.");
			if (string.IsNullOrEmpty(DeviceType))
				yield return new ValidationResult("The DeviceType field is required.");
		}
	}
}
