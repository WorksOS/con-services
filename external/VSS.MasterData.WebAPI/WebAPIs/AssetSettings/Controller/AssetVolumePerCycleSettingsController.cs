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
	[Route("v1/assetvolumepercyclesettings")]
	public class AssetVolumePerCycleSettingsController : ApiControllerBase
	{
		private readonly ILoggingService _loggingService;
		private readonly IAssetSettingsService<AssetSettingsRequestBase, AssetSettingsResponse> _assetSettingsService;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="assetSettingsService"></param>
		/// <param name="loggingService"></param>
		public AssetVolumePerCycleSettingsController(IAssetSettingsService<AssetSettingsRequestBase, AssetSettingsResponse> assetSettingsService, ILoggingService loggingService)
		{
			this._assetSettingsService = assetSettingsService;
			this._loggingService = loggingService;
			this._loggingService.CreateLogger(typeof(AssetVolumePerCycleSettingsController));
		}

		/// <summary>
		/// Fetch the mileage value for the given assets
		/// </summary>
		/// <param name="assetUIDs"></param>
		/// <returns></returns>
		[HttpPost]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(AssetVolumePerCycleSettingsResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(AssetVolumePerCycleSettingsResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(AssetVolumePerCycleSettingsResponse), (int)HttpStatusCode.InternalServerError)]
		// POST api/<controller>
		public async Task<ActionResult<AssetVolumePerCycleSettingsResponse>> Fetch(List<string> assetUIDs)
		{
			assetUIDs = await base.ReadRequestContentAsync(assetUIDs);

			var request = new AssetSettingsRequestBase
			{
				StartDate = DateTime.Parse(DateTime.UtcNow.ToShortDateString()),
				TargetValues = new Dictionary<AssetTargetType, double?> { { AssetTargetType.BucketVolumeinCuMeter, 0 } },
				CustomerUid = base.GetCustomerContext(Request),
				UserUid = base.GetUserContext(Request),
				AssetUIds = assetUIDs
			};

			this._loggingService.Debug("Fetching Asset Settings Bucket volume per cycle for the given request : " + JsonConvert.SerializeObject(request), MethodInfo.GetCurrentMethod().Name);

			var result = await this._assetSettingsService.Fetch(request);

			this._loggingService.Debug("Fetching Asset Settings Bucket volume per cycle response : " + JsonConvert.SerializeObject(result), MethodInfo.GetCurrentMethod().Name);

			return base.SendResponse(HttpStatusCode.OK, new AssetVolumePerCycleSettingsResponse(result.AssetSettingsLists, result.Errors.OfType<AssetErrorInfo>().ToList<AssetErrorInfo>()));
		}

		[HttpPut]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(AssetVolumePerCycleSettingsResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(AssetVolumePerCycleSettingsResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(AssetVolumePerCycleSettingsResponse), (int)HttpStatusCode.InternalServerError)]
		//[ChunkedEncodingFilter(typeof(AssetSettingsRequest), "request")]
		// POST api/<controller>
		public async Task<ActionResult<AssetVolumePerCycleSettingsResponse>> Save(AssetSettingsRequest request)
		{
			request = await base.ReadRequestContentAsync(request);

			AssetSettingsRequestBase serviceRequest = new AssetSettingsRequestBase();

			if (request != null)
			{
				serviceRequest = new AssetSettingsRequestBase
				{
					StartDate = DateTime.Parse(DateTime.UtcNow.ToShortDateString()),
					TargetValues = new Dictionary<AssetTargetType, double?> { { AssetTargetType.BucketVolumeinCuMeter, request.TargetValue } },
					CustomerUid = base.GetCustomerContext(Request),
					UserUid = base.GetUserContext(Request),
					AssetUIds = request.AssetUIds
				};
			}

			this._loggingService.Debug("Updating Asset Settings Volume Per Cycle for the given request : " + JsonConvert.SerializeObject(request), MethodInfo.GetCurrentMethod().Name);

			this._loggingService.Debug("Request : " + JsonConvert.SerializeObject(request), MethodInfo.GetCurrentMethod().Name);

			var result = await this._assetSettingsService.Save(serviceRequest);

			this._loggingService.Debug("Updating Asset Settings Bucket volume per cycle response : " + JsonConvert.SerializeObject(result), MethodInfo.GetCurrentMethod().Name);

			return base.SendResponse(HttpStatusCode.OK, new AssetVolumePerCycleSettingsResponse(result.AssetSettingsLists, result.Errors.OfType<AssetErrorInfo>().ToList<AssetErrorInfo>()));
		}
	}
}
