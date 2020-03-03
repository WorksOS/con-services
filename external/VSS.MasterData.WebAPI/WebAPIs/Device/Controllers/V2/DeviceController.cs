using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VSS.Authentication.JWT;
using VSS.MasterData.WebAPI.ClientModel.Device;
using VSS.MasterData.WebAPI.Filters;
using VSS.MasterData.WebAPI.Helpers;
using VSS.MasterData.WebAPI.Interfaces.Device;
using VSS.MasterData.WebAPI.KafkaModel.Device;

namespace VSS.MasterData.WebAPI.Device.Controllers.V2
{
	/// <summary>
	/// Get Device details in V2 payload format
	/// </summary>
	[Route("v2")]
	[ApiController]
	public class DeviceV2Controller : ControllerBase
	{
		#region Declarations
		private readonly ILogger _logger;
		private readonly Guid kiewitAMPCustomerUID;
		private readonly IDeviceService _deviceService;
		private readonly IConfiguration configuration;

		#endregion Declarations

		#region Constructors

		/// <param name="deviceService"></param>
		public DeviceV2Controller(IDeviceService deviceService, ILogger logger, IConfiguration configuration)
		{
			_deviceService = deviceService;
			this.configuration = configuration;
			_logger = logger;
			Guid.TryParse(configuration["KiewitAMPCustomerUID"], out kiewitAMPCustomerUID);
		}

		#endregion Constructors

		#region  Public Methods

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
		public IActionResult GetDeviceDetailsByDeviceUID([FromQuery]Guid deviceUID)
		{
			try
			{
				_logger.LogInformation($"Get DeviceDetails called for {deviceUID}");
				var userGuid = Utils.GetUserContext(Request);
				if (userGuid == null || _deviceService.ValidateAuthorizedCustomerByDevice(userGuid.Value, deviceUID))
				{
					_logger.LogInformation(string.Format("Unauthorized User Access-Get DeviceDetails called for {0}", deviceUID));
					return BadRequest("Unauthorized User Access");
				}

				var deviceData = _deviceService.GetDevicePropertiesV2ByDeviceGuid(deviceUID);
				if (deviceData != null && deviceData.Any())
				{
					var device =deviceData.FirstOrDefault();
					if (device != null)
					{
						var devicePayload = new
						{
							DeviceUID =device.DeviceUID,
							DeviceType= (device.DeviceType).ToString(),
							device.DeviceSerialNumber,
							DeviceState = ((DeviceStateEnum)device.DeviceState).ToString(),
							device.DataLinkType,
							device.DeregisteredUTC,
							device.ModuleType,
							Personalities = new List<DevicePersonalityPayload>()
						};
						foreach (var personality in deviceData)
						{
							if (!string.IsNullOrEmpty(personality.PersonalityValue))
							{
								devicePayload.Personalities.Add(new DevicePersonalityPayload
								{
									Name = ((DevicePersonalityTypeEnum)personality.PersonalityTypeId).ToString(),
									Description = personality.PersonalityDescription,
									Value = personality.PersonalityValue
								});
							}

						}
						return Ok(devicePayload);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogCritical("Get DeviceDetails threw an exception", ex);
				return StatusCode((int)HttpStatusCode.InternalServerError, ex);
			}
			return BadRequest("Device Doesn't Exist");
		}

		/// <summary>
		/// Get device details for an asset
		/// </summary>
		/// <param name="assetUID"></param>
		/// <param name="encodedJwt"></param>
		/// <returns></returns>
		[Route("Asset")]
		[UserUIDParser]
		[HttpGet]
		public IActionResult GetDeviceDetailsByAssetUID([FromQuery]Guid assetUID, [FromHeader(Name = "X-JWT-Assertion")] string encodedJwt)
		 {
			try
			{
				_logger.LogInformation($"Get DeviceDetails called for {assetUID}");

				var jwt = new TPaaSJWT(encodedJwt);
				
				if (jwt != null)
				{
					if (jwt.IsApplicationUserToken)
					{
						var userGuid = Utils.GetUserContext(Request);
						if (userGuid == null || _deviceService.ValidateAuthorizedCustomerByAsset(userGuid.Value, assetUID))
						{
							_logger.LogInformation(string.Format("Unauthorized User Access-Get DeviceDetails called for {0}", assetUID));
							return BadRequest("Unauthorized User Access");
						}
					}
					else // validate for kiewit customer
					{
						var customers = _deviceService.GetCustomersForApplication(jwt.ApplicationName);
						if (!customers.Any(x => x ==kiewitAMPCustomerUID))
						{
							_logger.LogInformation($"Application {jwt.ApplicationName} does not have mapped with {kiewitAMPCustomerUID}. Please contact your API administrator.");
							return BadRequest($"Application does not have Access. Please contact your API administrator.");
						}
					}
				}
				else
				{
					return BadRequest("JWT is invalid");
				}


				var assetDeviceDataList = _deviceService.GetDevicePropertiesV2ByAssetGuid(assetUID);
				if (assetDeviceDataList != null && assetDeviceDataList.Any())
				{
					var assetDevice = assetDeviceDataList.FirstOrDefault();
					if (assetDevice != null)
					{


						var assetDevicePayload = new
						{
							AssetUID = assetDevice.AssetUID,
							Devices = new List<DevicePropertiesPayLoad>()
						};
						var prevDeviceUid = Guid.Empty;
						var deviceIndex = -1;
						foreach (var assetDeviceData in assetDeviceDataList)
						{
							var deviceGuid = assetDeviceData.DeviceUID;
							if (prevDeviceUid != deviceGuid)
							{
								assetDevicePayload.Devices.Add(new DevicePropertiesPayLoad
								{
									DeviceUID = deviceGuid,
									DeviceType = assetDeviceData.DeviceType,
									DeviceSerialNumber = assetDeviceData.DeviceSerialNumber,
									DeviceState = ((DeviceStateEnum)assetDeviceData.DeviceState).ToString(),
									DataLinkType = assetDeviceData.DataLinkType,
									DeregisteredUTC = assetDeviceData.DeregisteredUTC,
									ModuleType = assetDeviceData.ModuleType,
									Personalities = new List<DevicePersonalityPayload>()
								});
								prevDeviceUid = deviceGuid;
								deviceIndex++;
							}
							assetDevicePayload.Devices[deviceIndex].Personalities.Add(new DevicePersonalityPayload
							{
								Name = ((DevicePersonalityTypeEnum)assetDeviceData.PersonalityTypeId).ToString(),
								Description = assetDeviceData.PersonalityDescription,
								Value = assetDeviceData.PersonalityValue
							});
						}
						return Ok(assetDevicePayload);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError("Get DeviceDetails threw an exception", ex);
				return StatusCode((int)HttpStatusCode.InternalServerError, ex);
			}
			return BadRequest("Device Doesn't Exist");
		}
		#endregion  Public Methods


	}
}