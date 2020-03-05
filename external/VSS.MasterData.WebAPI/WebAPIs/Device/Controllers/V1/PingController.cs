using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using ClientModel.DeviceConfig.Request;
using ClientModel.DeviceConfig.Response.DeviceConfig.Asset_Security;
using ClientModel.DeviceConfig.Response.DeviceConfig.Ping;
using CommonApiLibrary.Filters;
using CommonModel.DeviceSettings.ConfigNameValues;
using CommonModel.Error;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Device.DeviceConfig;

namespace VSS.MasterData.WebAPI.Device.Controllers.V1
{
	/// <summary>
	/// 
	/// </summary>
	[Route("v1/ping")]
	public class PingController: DeviceConfigApiControllerBase
	{
		//private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly ILogger _loggingService;
		private readonly IDevicePingService _devicePingService;
		private readonly IMessageConstructor _messageConstructor;
		private readonly IEnumerable<IRequestValidator<IServiceRequest>> _validators;
		private readonly string className = "PingController";

		/// <summary>
		/// 
		/// </summary>
		/// <param name="devicePingService"></param>
		/// <param name="attributeMaps"></param>
		/// <param name="loggingService"></param>
		public PingController(IDevicePingService devicePingService, DeviceConfigAttributeToRequestMaps attributeMaps, ILogger loggingService): base(attributeMaps, loggingService)
		{
			_devicePingService = devicePingService;
			_loggingService = loggingService;
			//_loggingService.CreateLogger(typeof(PingController));
		}

		// GET
		/// <summary>
		/// </summary>
		/// <param name="request">Asset Guid and Device Guid</param>
		/// <remarks>GET Ping Request Status</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		/// <response code="500">Internal Server Error</response>
		[HttpGet]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(DeviceConfigAssetSecurityResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(DeviceConfigAssetSecurityResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(DeviceConfigAssetSecurityResponse), (int)HttpStatusCode.InternalServerError)]
		public async Task<ActionResult> GetPingRequestStatus([FromQuery] DevicePingLogRequest request)
		{
			request = await base.ReadRequestContentAsync<DevicePingLogRequest>(request);

			this._loggingService.LogInformation(string.Format("Fetching Ping Request Status for AssetUID {0} and DeviceUID {1}", request.AssetUID, request.DeviceUID), className);

			if (request == null)
			{
				this._loggingService.LogInformation("Invalid Request", "PingController.PostDevicePingRequest");
				IList<IErrorInfo> errors = new List<IErrorInfo>();
				errors.Add(new ErrorInfo() { Message = Utils.GetEnumDescription(ErrorCodes.RequestInvalid), ErrorCode = (int)ErrorCodes.RequestInvalid });
				return Ok(new DevicePingStatusResponse() { Errors = errors });
			}

			DevicePingStatusResponse devicePingStatusResponse = new DevicePingStatusResponse();

			request.UserUID = base.GetUserContext(Request);
			request.CustomerUID = base.GetCustomerContext(Request);

			this._loggingService.LogInformation("Started invoking Device Ping Log Service with request : " + JsonConvert.SerializeObject(request), "PingController.GetPingRequestStatus");

			devicePingStatusResponse = await _devicePingService.GetPingRequestStatus(request);

			this._loggingService.LogInformation("Ended invoking Device Ping Log Service with response : " + JsonConvert.SerializeObject(devicePingStatusResponse), "PingController.GetPingRequestStatus");
			if (devicePingStatusResponse == null)
			{
				IList<IErrorInfo> errors = new List<IErrorInfo>();
				errors.Add(new ErrorInfo() { Message = Utils.GetEnumDescription(ErrorCodes.PingRequestNotFound), ErrorCode = (int)ErrorCodes.PingRequestNotFound });
				return Ok(new DevicePingStatusResponse() { AssetUID = request.AssetUID.ToString(), DeviceUID = request.DeviceUID.ToString(), Errors = errors });
			}
			else
			{
				return Ok(devicePingStatusResponse);
			}
		}


		// POST
		/// <summary>
		/// </summary>
		/// <param name="devicePingLogRequest">DevicePingLogRequest</param>
		/// <remarks>Save Device Ping Request</remarks>
		/// <response code="200">Ok</response>
		/// <response code="400">Bad request</response>
		/// <response code="500">Internal Server Error</response>
		[Route("")]
		[HttpPost]
		[UserUidParser]
		[ProducesResponseType(typeof(DeviceConfigAssetSecurityResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(DeviceConfigAssetSecurityResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(DeviceConfigAssetSecurityResponse), (int)HttpStatusCode.InternalServerError)]		 
		public async Task<ActionResult> PostDevicePingRequest(DevicePingLogRequest devicePingLogRequest)
		{
			devicePingLogRequest = await base.ReadRequestContentAsync<DevicePingLogRequest>(devicePingLogRequest);

			if (devicePingLogRequest == null)
			{
				this._loggingService.LogInformation("Invalid Request", "PingController.PostDevicePingRequest");
				IList<IErrorInfo> errors = new List<IErrorInfo>();
				errors.Add(new ErrorInfo() { Message = Utils.GetEnumDescription(ErrorCodes.RequestInvalid), ErrorCode = (int)ErrorCodes.RequestInvalid });
				return Ok(new DevicePingStatusResponse() { Errors = errors });
			}

			this._loggingService.LogInformation("Saving Device Ping Request for request : " + JsonConvert.SerializeObject(devicePingLogRequest), "PingController.Post");

			var devicePingStatusResponse = new DevicePingStatusResponse();

			devicePingLogRequest.UserUID = base.GetUserContext(Request);
			devicePingLogRequest.CustomerUID = base.GetCustomerContext(Request);

			this._loggingService.LogInformation(string.Format("Post Ping Request to Device Ping Log Table for Asset = {0} and Device = {1}", devicePingLogRequest.AssetUID, devicePingLogRequest.DeviceUID), "PingController.Post");
			devicePingStatusResponse = await _devicePingService.PostDevicePingRequest(devicePingLogRequest);
			if (devicePingStatusResponse == null)
			{
				this._loggingService.LogInformation("Not saved Successfully", "PingController.PostDevicePingRequest");
				IList<IErrorInfo> errors = new List<IErrorInfo>();
				errors.Add(new ErrorInfo() { Message = Utils.GetEnumDescription(ErrorCodes.PingRequestNotSaved), ErrorCode = (int)ErrorCodes.PingRequestNotSaved });
				return StatusCode((int)HttpStatusCode.InternalServerError, new DevicePingStatusResponse() { AssetUID = devicePingLogRequest.AssetUID.ToString(), DeviceUID = devicePingLogRequest.DeviceUID.ToString(), Errors = errors });
			}
			else
			{
				return Ok(devicePingStatusResponse);
			}
		}
	}
}
