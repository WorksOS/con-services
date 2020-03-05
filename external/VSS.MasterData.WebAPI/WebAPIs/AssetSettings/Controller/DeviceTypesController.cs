using ClientModel.AssetSettings.Request;
using ClientModel.AssetSettings.Response;
using CommonApiLibrary;
using CommonApiLibrary.Filters;
using Infrastructure.Service.AssetSettings.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.Logging;

namespace AssetSettings.Controller
{
	/// <summary>
	/// To fetch all the device types for the associated customer and user
	/// </summary>
	[Route("v1/devicetypes")]
	public class DeviceTypesController : ApiControllerBase
	{
		private readonly ILoggingService _loggingService;
		private readonly IAssetSettingsListService _assetSettingsListService;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetSettingsService"></param>
		/// <param name="loggingService"></param>
		public DeviceTypesController(IAssetSettingsListService assetSettingsListService, ILoggingService loggingService)
		{
			this._assetSettingsListService = assetSettingsListService;
			this._loggingService = loggingService;
			this._loggingService.CreateLogger<DeviceTypesController>();
		}

		[HttpPost]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(DeviceTypeListResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(DeviceTypeListResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(DeviceTypeListResponse), (int)HttpStatusCode.InternalServerError)]
		public async Task<ActionResult<DeviceTypeListResponse>> FetchDeviceTypes(AssetDeviceTypeRequest request, [FromQuery] string subAccountCustomerUid = null, [FromQuery] bool isSwitch = false)
		{
			Guid parsedSubAccountCustomerUid;

			request = await base.ReadRequestContentAsync(request);

			if (request == null)
				request = new AssetDeviceTypeRequest();
			request.CustomerUid = base.GetCustomerContext(Request);
			request.UserUid = base.GetUserContext(Request);
			request.IsSwitchRequest = isSwitch;

			if (!string.IsNullOrEmpty(subAccountCustomerUid) && Guid.TryParse(subAccountCustomerUid, out parsedSubAccountCustomerUid))
			{
				request.SubAccountCustomerUid = parsedSubAccountCustomerUid.ToString("N");
			}
			if (request.AssetUIDs != null && request.AssetUIDs.Count > 0)
			{
				this._loggingService.Info("Asset UIDs Count : " + request.AssetUIDs.Count, MethodInfo.GetCurrentMethod().Name);
			}
			this._loggingService.Info("Fetching Device Types for the given request : " + JsonConvert.SerializeObject(request), MethodInfo.GetCurrentMethod().Name);
			var result = await this._assetSettingsListService.FetchDeviceTypes(request);
			this._loggingService.Debug("Fetching Device Types response : " + JsonConvert.SerializeObject(result), MethodInfo.GetCurrentMethod().Name);
			return base.SendResponse(HttpStatusCode.OK, new DeviceTypeListResponse(result));
		}
	}
}
