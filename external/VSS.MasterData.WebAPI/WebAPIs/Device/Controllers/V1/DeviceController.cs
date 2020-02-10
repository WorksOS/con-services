using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.MasterData.WebAPI.ClientModel.Device;
using VSS.MasterData.WebAPI.DbModel.Device;
using VSS.MasterData.WebAPI.Filters;
using VSS.MasterData.WebAPI.Helpers;
using VSS.MasterData.WebAPI.Interfaces;
using VSS.MasterData.WebAPI.Interfaces.Device;
using VSS.MasterData.WebAPI.KafkaModel.Device;

namespace VSS.MasterData.WebAPI.Device.Controllers.V1
{
	/// <summary>
	/// Perform Device CRUD , Association, Transfer or replacement of device from Assets
	/// </summary>
	[Route("v1")]
	[ApiController]
	public class DeviceV1Controller : ControllerBase
	{
		#region Declarations

		private readonly ILogger logger;
		private readonly IDeviceService deviceService;
		private readonly Dictionary<string, DbDeviceType> deviceTypesCache = new Dictionary<string, DbDeviceType>(StringComparer.InvariantCultureIgnoreCase);
		private static readonly List<string> PLDeviceTypes = new List<string> { "pl121", "pl321" };


		#endregion Declarations

		#region Constructors

		/// <param name="deviceService"></param>
		/// <param name="logger"></param>
		/// <param name="deviceTypeService"></param>
		public DeviceV1Controller(IDeviceService deviceService, ILogger logger, IDeviceTypeService deviceTypeService)
		{
			this.deviceService = deviceService;
			this.logger = logger;
			this.deviceTypesCache = deviceTypeService.GetDeviceType();
		}

		#endregion Constructors

		#region Public Methods

		// POST: api/device
		/// <summary>
		/// Create device
		/// </summary>
		/// <param name="device">CreateDeviceEvent model</param>
		/// <remarks>Create new Device</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("")]
		[HttpPost]
		public IActionResult CreateDevice([FromBody] CreateDeviceEvent device)
		{
			try
			{
				logger.LogInformation($"CreateDevice called for Device {device.DeviceUID}");

				if (device.DeviceSerialNumber == null)
				{
					logger.LogInformation(
						$"Received CreateDevicePayload with DeviceSerialNumber as null for DeviceType {device.DeviceType} and DeviceUID {device.DeviceUID} ");
					return BadRequest("Device SerialNumber can't be null");
				}

				if (!deviceTypesCache.TryGetValue(device.DeviceType, out DbDeviceType deviceType))
				{
					logger.LogWarning($"DeviceType {device.DeviceType} is not exists");
					return BadRequest("Make sure request contains valid DeviceType");
				}

				if (!Enum.TryParse<DeviceStateEnum>(device.DeviceState?.Trim(), true, out DeviceStateEnum deviceState))
				{
					return BadRequest("Invalid Device state. Supported Device states are Installed/Provisioned/Subscribed/DeregisteredTechnician/DeregisteredStore");
				}

				if (deviceService.CheckExistingDevice(device.DeviceUID.Value))
				{
					return BadRequest("Device already exists!");
				}

				device.ReceivedUTC = DateTime.UtcNow;
				if (deviceService.CreateDevice(device, deviceState))
				{
					return Ok();
				}
				else
				{
					return BadRequest("Create Request is not processed.");
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		// PUT: api/device
		/// <summary>
		/// Update Device
		/// </summary>
		/// <param name="device">UpdateDeviceEvent model</param>
		/// <remarks>Updates existing device</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("")]
		[HttpPut]
		public IActionResult UpdateDevice([FromBody] UpdateDeviceEvent device)
		{
			try
			{
				logger.LogInformation($"UpdateDevice called for Device {device.DeviceUID}");

				if (!(device.OwningCustomerUID == null || device.OwningCustomerUID == Guid.Empty)
					|| !(device.DeregisteredUTC.HasValue ? device.DeregisteredUTC.Value.ToString("MM-dd-yyyy") == Constants.INVALID_DATE_TIME_VALUE
							: device.DeregisteredUTC == new DateTime(1111, 11, 11))
					|| device.ModuleType != null
					|| device.MainboardSoftwareVersion != null
					|| device.RadioFirmwarePartNumber != null
					|| device.GatewayFirmwarePartNumber != null
					|| device.DataLinkType != null
					|| device.DeviceState != null
					|| device.FirmwarePartNumber != null
					|| device.CellModemIMEI != null
					|| device.DevicePartNumber != null
					|| device.CellularFirmwarePartnumber != null
					|| device.NetworkFirmwarePartnumber != null
					|| device.SatelliteFirmwarePartnumber != null)
				{
					if (!string.IsNullOrEmpty(device.DeviceType) && !deviceTypesCache.TryGetValue(device.DeviceType, out DbDeviceType deviceType))
					{
						logger.LogWarning($"DeviceType {device.DeviceType} is not exists");
						return BadRequest("Make sure request contains valid DeviceType");
					}

					DeviceStateEnum deviceState = DeviceStateEnum.None;
					if (!string.IsNullOrEmpty(device.DeviceState) && !Enum.TryParse<DeviceStateEnum>(device.DeviceState?.Trim(), true, out deviceState))
					{
						return BadRequest("Invalid Device State for updation. Supported Device states are Installed/Provisioned/Subscribed/DeregisteredTechnician/DeregisteredStore");
					}

					if (!deviceService.CheckExistingDevice(device.DeviceUID.Value))
					{
						return BadRequest("Device doesn't exist!");
					}
					device.ReceivedUTC = DateTime.UtcNow;
					if (deviceService.UpdateDevice(device, deviceState))
					{
						return Ok("Update Request is processed successfully");
					}
					else
					{
						return Ok("Update Request is processed and no data to be updated.");
					}
				}
				else
				{
					return BadRequest("Update Request should have atleast one data to update");
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		// PUT: api/device
		/// <summary>
		/// Update Device
		/// </summary>
		/// <param name="device">UpdateDeviceProperties model</param>
		/// <remarks>Updates existing device</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("UpdateDeviceProperties")]
		[HttpPut]
		public IActionResult UpdateDeviceProperties([FromBody] UpdateDeviceProperties device)
		{
			try
			{
				logger.LogInformation($"UpdateDeviceProperties called for Device {device.DeviceSerialNumber}-{device.DeviceType}");

				if (device.ModuleType!=null
					|| device.MainboardSoftwareVersion != null
				  || device.RadioFirmwarePartNumber != null
				  || device.GatewayFirmwarePartNumber != null
				  || device.DataLinkType != null
				  || device.CellularFirmwarePartnumber != null
				  || device.NetworkFirmwarePartnumber != null
				  || device.SatelliteFirmwarePartnumber != null)
				{
					if (!deviceTypesCache.TryGetValue(device.DeviceType, out DbDeviceType deviceType))
					{
						logger.LogWarning($"DeviceType {device.DeviceType} is not exists");
						return BadRequest("Make sure request contains valid DeviceType");
					}

					var deviceUids = deviceService.GetDeviceDetailsBySerialNumberAndType(device.DeviceSerialNumber, device.DeviceType) ?? new List<Guid>();

					if (deviceUids != null && deviceUids.Count() > 1) // two devices for same serialnumber
					{
						logger.LogWarning($"Two Devices are with same SerailNumber {device.DeviceSerialNumber} & DeviceType {device.DeviceType}");
						return Ok();
					}
					if (!deviceUids.Any())
						return BadRequest("Device doesn't exist!");

					if (deviceService.UpdateDeviceProperties(device, deviceUids.First()))
					{
						return Ok();
					}
					else
					{
						return BadRequest("No properties to update");
					}
				}
				else
				{
					return BadRequest("UpdateDeviceProperties should have atleast one data to update");
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}

		// GET
		/// <summary>
		/// </summary>
		/// <param name="deviceUID">Device Guid</param>
		/// <remarks>GET DeviceDetails</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("")]
		[UserUIDParser]
		[HttpGet]
		public IActionResult GetDeviceDetailsByDeviceUID(Guid deviceUID)
		{
			var deviceData = new DeviceProperties();
			try
			{
				logger.LogInformation($"Get DeviceDetails called for {deviceUID}");
				var userGuid = Utils.GetUserContext(Request);
				if (userGuid == null || deviceService.ValidateAuthorizedCustomerByDevice(userGuid.Value, deviceUID))
				{
					logger.LogInformation(string.Format("Unauthorized User Access-Get DeviceDetails called for {0}", deviceUID));
					return BadRequest("Unauthorized User Access");
				}
				deviceData = deviceService.GetExistingDeviceProperties(deviceUID);
				
				if (deviceData != null)
				{
					if (PLDeviceTypes.Contains(deviceData.DeviceType.ToLower(), StringComparer.InvariantCultureIgnoreCase))
					{
						deviceData.RadioFirmwarePartNumber = deviceData.FirmwarePartNumber;
						deviceData.FirmwarePartNumber = null;
					}
					return Ok(deviceData);
				}
				else
				{
					return BadRequest("Device Doesn't Exist");
				}
			}
			catch (Exception ex)
			{
				logger.LogError("Get DeviceDetails threw an exception", ex);
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}

		}

		/// <summary>
		/// Associate Device Asset
		/// </summary>
		/// <param name="associateDeviceAsset">AssociateDeviceAsset model</param>
		/// <remarks>Associate Device And Asset</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("AssociateDeviceAsset")]
		[HttpPost]
		public IActionResult AssociateDeviceAsset([FromBody] AssociateDeviceAssetEvent associateDeviceAsset)
		{
			try
			{
				logger.LogInformation($"AssociateDeviceAsset called for Asset {associateDeviceAsset.AssetUID} Device {associateDeviceAsset.DeviceUID}");

				var associateDeviceDetail = deviceService.GetAssetDevice(associateDeviceAsset.AssetUID, associateDeviceAsset.DeviceUID);
				if (associateDeviceDetail != null)
				{
					if (associateDeviceDetail.ActionUTC >= associateDeviceAsset.ActionUTC)
					{
						return BadRequest("The AssociateDeviceAsset does not have the latest data to be updated.");
					}
					else
					{
						return BadRequest("The Device is already Associated with this Asset.");
					}

				}
				//To check any device are dissociated for this asset
				var associateDevice = deviceService.GetAssociatedDevicesByAsset(associateDeviceAsset.AssetUID);
				if (associateDevice != null)
				{
					if (associateDevice.DeviceStatusID == DeviceStateEnum.Subscribed.GetHashCode())
					{
						logger.LogInformation(string.Format("AssociateDeviceAsset ignored. Already Asset ({0}) associated with Device ({1})"
							, associateDeviceAsset.AssetUID, associateDevice.DeviceUID));
						return Ok("AssociateDeviceAsset ignored as another device is subscribed already");
					}
					else
					{
						var dissociateDeviceAsset = new DissociateDeviceAssetEvent()
						{
							AssetUID = new Guid(associateDeviceAsset.AssetUID.ToString()),
							DeviceUID = associateDevice.DeviceUID,
							ActionUTC = associateDeviceAsset.ActionUTC.Value.AddMilliseconds(-1),
							ReceivedUTC = DateTime.UtcNow
						};

						deviceService.DissociateAssetDevice(dissociateDeviceAsset);
					}
				}


				if (deviceService.AssociateAssetDevice(associateDeviceAsset))
				{
					return Ok();
				}
				else
				{
					return StatusCode((int)HttpStatusCode.InternalServerError, "AssociateDeviceAsset is not published.");
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}



		/// <summary>
		/// Dissociate Device Asset
		/// </summary>
		/// <param name="dissociateDeviceAsset">DissociateDeviceAssetEvent model</param>
		/// <remarks>Dissociate Device And Asset</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		[Route("DissociateDeviceAsset")]
		[HttpPost]
		public IActionResult DissociateDeviceAsset([FromBody] DissociateDeviceAssetEvent dissociateDeviceAsset)
		{
			try
			{
				logger.LogInformation($"DissociateDeviceAsset called for Asset {dissociateDeviceAsset.AssetUID} Device {dissociateDeviceAsset.DeviceUID}");

				var associateDeviceDetail = deviceService.GetAssetDevice(dissociateDeviceAsset.AssetUID, dissociateDeviceAsset.DeviceUID);
				if (associateDeviceDetail == null)
				{
					return BadRequest("No AssetDevice Association exists for the given Asset-Device.");

				}
				else if (associateDeviceDetail.ActionUTC >= dissociateDeviceAsset.ActionUTC)
				{
					return BadRequest("The DissociateDeviceAssetEvent does not have the latest data to be updated.");
				}

				if (deviceService.DissociateAssetDevice(dissociateDeviceAsset))
				{
					return Ok();
				}
				else
				{
					return StatusCode((int)HttpStatusCode.InternalServerError, "DissociateDeviceAsset is not processed.");
				}

			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message + ex.StackTrace);
				return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
			}
		}
		#endregion Public Methods
	}
}