using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VSS.MasterData.WebAPI.ClientModel.Device
{
	public class DeviceEventBase : IValidatableObject
	{
		/// <summary>
		/// Device unique identifier
		/// </summary>
		[Required]
		public Guid? DeviceUID { get; set; }
		[StringLength(128)]
		/// <summary>
		/// Device Serial Number
		/// </summary>
		public string DeviceSerialNumber { get; set; }

		/// <summary>
		/// Device deregistered UTC time
		/// </summary>
		public DateTime? DeregisteredUTC { get; set; }

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

		[StringLength(50)]
		/// <summary>
		/// Device Radio firmware part number
		/// </summary>
		public string RadioFirmwarePartNumber { get; set; }

		[StringLength(50)]
		/// <summary>
		/// Device Gateway firmware part number
		/// </summary>
		public string GatewayFirmwarePartNumber { get; set; }

		[StringLength(50)]
		/// <summary>
		/// Device data link type
		/// </summary>
		public string DataLinkType { get; set; }

		[StringLength(50)]
		/// <summary>
		/// Device firmware part number
		/// </summary>
		public string FirmwarePartNumber { get; set; }

		[StringLength(50)]
		/// <summary>
		/// Device cell modem IMEI
		/// </summary>
		public string CellModemIMEI { get; set; }

		[StringLength(50)]
		/// <summary>
		/// Device part number
		/// </summary>
		public string DevicePartNumber { get; set; }

		[StringLength(50)]
		/// <summary>
		/// Device cellular firmware part number
		/// </summary>
		public string CellularFirmwarePartnumber { get; set; }

		[StringLength(50)]
		/// <summary>
		/// Device network firmware part number
		/// </summary>
		public string NetworkFirmwarePartnumber { get; set; }

		[StringLength(50)]
		/// <summary>
		/// Device Satellite firmware part number
		/// </summary>
		public string SatelliteFirmwarePartnumber { get; set; }

		/// <summary>
		/// Create Device event Action UTC time
		/// </summary>
		public DateTime? ActionUTC { get; set; }

		/// <summary>
		/// Create Device event received UTC time
		/// </summary>
		public DateTime ReceivedUTC { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (DeviceUID == null || DeviceUID == Guid.Empty)
				yield return new ValidationResult("The DeviceUID field is required.");
		}
	}
}