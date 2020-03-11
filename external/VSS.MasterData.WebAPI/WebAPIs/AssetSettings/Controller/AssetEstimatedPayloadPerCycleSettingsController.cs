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
	/// <summary>
	/// This controller has all the methods to save and fetch Estimated payload per cycle information for assets.
	/// </summary>
	[Route("v1/assetestimatedpayloadpercyclesettings")]
	public class AssetEstimatedPayloadPerCycleSettingsController : ApiControllerBase
	{
		private readonly ILoggingService _loggingService;
		private readonly IAssetSettingsService<AssetSettingsRequestBase, AssetSettingsResponse> _assetSettingsService;

		/// <summary>
		/// Injecting required params in class.
		/// </summary>
		/// <param name="assetSettingsService"></param>
		/// <param name="loggingService"></param>
		public AssetEstimatedPayloadPerCycleSettingsController(IAssetSettingsService<AssetSettingsRequestBase, AssetSettingsResponse> assetSettingsService, ILoggingService loggingService)
		{
			this._assetSettingsService = assetSettingsService;
			this._loggingService = loggingService;
			this._loggingService.CreateLogger<AssetEstimatedPayloadPerCycleSettingsController>();
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <response code="200">Saves Asset setting Estimated Payload per cycle for the AssetIds provided</response>
		/// <response code="400">When Invalid Parameters Passed</response>
		/// <returns></returns>
		[HttpPut]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(AssetEstimatedPayloadPerCycleSettingsResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(AssetEstimatedPayloadPerCycleSettingsResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(AssetEstimatedPayloadPerCycleSettingsResponse), (int)HttpStatusCode.InternalServerError)]
		// POST api/<controller>
		public async Task<ActionResult<AssetEstimatedPayloadPerCycleSettingsResponse>> Save(AssetSettingsRequest request)
		{
			request = await base.ReadRequestContentAsync(request);

			AssetSettingsRequestBase serviceRequest = new AssetSettingsRequestBase();

			if (request != null)
			{
				serviceRequest = new AssetSettingsRequestBase
				{
					StartDate = DateTime.Parse(DateTime.UtcNow.ToShortDateString()),
					TargetValues = new Dictionary<AssetTargetType, double?> { { AssetTargetType.PayloadPerCycleInTonnes, request.TargetValue } },
					CustomerUid = base.GetCustomerContext(Request),
					UserUid = base.GetUserContext(Request),
					AssetUIds = request.AssetUIds
				};
			}

			this._loggingService.Debug("Updating Asset Settings Estimated Payload Per Cycle for the given request : " + JsonConvert.SerializeObject(request), MethodInfo.GetCurrentMethod().Name);

			this._loggingService.Debug("Request : " + JsonConvert.SerializeObject(request), MethodInfo.GetCurrentMethod().Name);

			var result = await this._assetSettingsService.Save(serviceRequest);

			this._loggingService.Debug("Updating Asset Settings Estimated Payload per cycle response : " + JsonConvert.SerializeObject(result), MethodInfo.GetCurrentMethod().Name);

			return base.SendResponse(HttpStatusCode.OK, new AssetEstimatedPayloadPerCycleSettingsResponse(result.AssetSettingsLists, result.Errors.OfType<AssetErrorInfo>().ToList<AssetErrorInfo>()));
		}


		/// <summary>
		/// This method returns Estimated Payload per cycle for all the requested assets.
		/// </summary>
		/// <response code="200">Returns Asset setting Estimated Payload per cycle for the AssetIds provided</response>
		/// <response code="400">When Invalid Parameters Passed</response>
		/// <returns></returns>
		[HttpPost]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(AssetEstimatedPayloadPerCycleSettingsResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(AssetEstimatedPayloadPerCycleSettingsResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(AssetEstimatedPayloadPerCycleSettingsResponse), (int)HttpStatusCode.InternalServerError)]
		// POST api/<controller>
		public async Task<ActionResult<AssetEstimatedPayloadPerCycleSettingsResponse>> Fetch(List<string> assetUIDs)
		{
			assetUIDs = await base.ReadRequestContentAsync(assetUIDs);

			var request = new AssetSettingsRequestBase
			{
				StartDate = DateTime.Parse(DateTime.UtcNow.ToShortDateString()),
				TargetValues = new Dictionary<AssetTargetType, double?> { { AssetTargetType.PayloadPerCycleInTonnes, 0 } },
				CustomerUid = base.GetCustomerContext(Request),
				UserUid = base.GetUserContext(Request),
				AssetUIds = assetUIDs
			};

			this._loggingService.Debug("Fetching Asset Settings Estimated Payload per cycle for the given request : " + JsonConvert.SerializeObject(request), MethodInfo.GetCurrentMethod().Name);

			var result = await this._assetSettingsService.Fetch(request);

			this._loggingService.Debug("Fetching Asset Settings Estimated Payload per cycle response : " + JsonConvert.SerializeObject(result), MethodInfo.GetCurrentMethod().Name);

			return base.SendResponse(HttpStatusCode.OK, new AssetEstimatedPayloadPerCycleSettingsResponse(result.AssetSettingsLists, result.Errors.OfType<AssetErrorInfo>().ToList<AssetErrorInfo>()));
		}
	}
}
