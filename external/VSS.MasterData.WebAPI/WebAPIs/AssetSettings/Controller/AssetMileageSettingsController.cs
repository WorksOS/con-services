using ClientModel.AssetSettings.Request;
using ClientModel.AssetSettings.Response.AssetTargets;
using CommonApiLibrary;
using CommonApiLibrary.Filters;
using CommonModel.Enum;
using CommonModel.Error;
using Infrastructure.Service.AssetSettings.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.Logging;

namespace AssetSettings.Controller
{
	[Route("v1/assetmileagesettings")]
	public class AssetMileageSettingsController : ApiControllerBase
	{
		private readonly ILoggingService _loggingService;
		private readonly IAssetSettingsService<AssetSettingsRequestBase, AssetSettingsResponse> _assetSettingsService;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetSettingsService"></param>
		/// <param name="loggingService"></param>
		public AssetMileageSettingsController(IAssetSettingsService<AssetSettingsRequestBase, AssetSettingsResponse> assetSettingsService, ILoggingService loggingService)
		{
			this._assetSettingsService = assetSettingsService;
			this._loggingService = loggingService;
			this._loggingService.CreateLogger(this.GetType());
		}

		/// <summary>
		/// Fetch the Mileage Asset Settings for the given AssetUIDs
		/// </summary>
		/// <param name="assetUIDs"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(AssetMileageSettingsResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(AssetMileageSettingsResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(AssetMileageSettingsResponse), (int)HttpStatusCode.InternalServerError)]
		// POST api/<controller>
		public async Task<ActionResult<AssetMileageSettingsResponse>> Fetch(List<String> assetUIDs = null)
		{
			assetUIDs = await base.ReadRequestContentAsync(assetUIDs);

			var request = new AssetSettingsRequestBase
			{
				StartDate = DateTime.Parse(DateTime.UtcNow.ToShortDateString()),
				TargetValues = new Dictionary<AssetTargetType, double?> { { AssetTargetType.OdometerinKmsPerWeek, 0 } },
				CustomerUid = base.GetCustomerContext(Request),
				UserUid = base.GetUserContext(Request),
				AssetUIds = assetUIDs
			};

			this._loggingService.Info("Fetching Asset Settings Mileage for the given request : " + JsonConvert.SerializeObject(request), MethodInfo.GetCurrentMethod().Name);
			var result = await this._assetSettingsService.Fetch(request);
			this._loggingService.Debug("Fetching Asset Settings Mileage response : " + JsonConvert.SerializeObject(result), MethodInfo.GetCurrentMethod().Name);

			return SendResponse(HttpStatusCode.OK, new AssetMileageSettingsResponse(result.AssetSettingsLists, result.Errors.OfType<AssetErrorInfo>().ToList<AssetErrorInfo>()));
		}

		[HttpPut]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(AssetMileageSettingsResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(AssetMileageSettingsResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(AssetMileageSettingsResponse), (int)HttpStatusCode.InternalServerError)]
		// POST api/<controller>
		public async Task<ActionResult<AssetMileageSettingsResponse>> Save(AssetSettingsRequest request)
		{
			request = await base.ReadRequestContentAsync(request);

			AssetSettingsRequestBase serviceRequest = new AssetSettingsRequestBase();

			if (request != null)
			{
				serviceRequest = new AssetSettingsRequestBase
				{
					StartDate = DateTime.Parse(DateTime.UtcNow.ToShortDateString()),
					TargetValues = new Dictionary<AssetTargetType, double?> { { AssetTargetType.OdometerinKmsPerWeek, request.TargetValue } },
					CustomerUid = base.GetCustomerContext(Request),
					UserUid = base.GetUserContext(Request),
					AssetUIds = request.AssetUIds
				};
			}

			this._loggingService.Debug("Updating Asset Settings Mileage for the given request : " + JsonConvert.SerializeObject(request), MethodInfo.GetCurrentMethod().Name);

			this._loggingService.Debug("Request : " + JsonConvert.SerializeObject(request), MethodInfo.GetCurrentMethod().Name);

			var result = await this._assetSettingsService.Save(serviceRequest);

			this._loggingService.Debug("Updating Asset Settings Mileage response : " + JsonConvert.SerializeObject(result), MethodInfo.GetCurrentMethod().Name);

			return SendResponse(HttpStatusCode.OK, new AssetMileageSettingsResponse(result.AssetSettingsLists, result.Errors.OfType<AssetErrorInfo>().ToList<AssetErrorInfo>()));
		}
	}
}