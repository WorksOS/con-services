using ClientModel.AssetSettings.Request.AssetSettings;
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
	[Route("v1/assetfuelburnratesettings")]
	public class AssetFuelBurnRateSettingsController : ApiControllerBase
	{
		private readonly ILoggingService _loggingService;
		private readonly IAssetSettingsService<AssetFuelBurnRateSettingRequest, AssetFuelBurnRateSettingsDetails> _assetSettingsService;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="injectConfig"></param>
		/// <param name="loggingService"></param>
		public AssetFuelBurnRateSettingsController(IAssetSettingsService<AssetFuelBurnRateSettingRequest, AssetFuelBurnRateSettingsDetails> assetSettingsService, ILoggingService loggingService)
		{
			this._assetSettingsService = assetSettingsService;
			this._loggingService = loggingService;
			this._loggingService.CreateLogger(typeof(AssetFuelBurnRateSettingsController));
		}

		/// <summary>
		/// Fetch the asset fuel burn rate value for the given assets
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(AssetFuelBurnRateSettingsResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(AssetFuelBurnRateSettingsResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(AssetFuelBurnRateSettingsResponse), (int)HttpStatusCode.InternalServerError)]
		// POST api/<controller>
		public async Task<ActionResult<AssetFuelBurnRateSettingsResponse>> Fetch(List<String> assetUIDs = null)
		{
			assetUIDs = await base.ReadRequestContentAsync(assetUIDs);

			var request = new AssetFuelBurnRateSettingRequest
			{
				StartDate = DateTime.Parse(DateTime.UtcNow.ToShortDateString()),
				CustomerUid = base.GetCustomerContext(Request),
				UserUid = base.GetUserContext(Request),
				AssetUIds = assetUIDs,
				IdleTargetValue = 0,
				WorkTargetValue = 0,
				TargetValues = new Dictionary<AssetTargetType, double?>
					{
						{ AssetTargetType.IdlingBurnRateinLiPerHour, 0 },
						{ AssetTargetType.WorkingBurnRateinLiPerHour, 0 }
					}
			};

			this._loggingService.Info("Fetching Asset Settings Burn Rate for the given request : " + JsonConvert.SerializeObject(request), MethodInfo.GetCurrentMethod().Name);
			var result = await this._assetSettingsService.Fetch(request);
			this._loggingService.Debug("Fetching Asset Settings Burn Rate response : " + JsonConvert.SerializeObject(result), MethodInfo.GetCurrentMethod().Name);

			return base.SendResponse(HttpStatusCode.OK, new AssetFuelBurnRateSettingsResponse(result.AssetSettingsLists, result.Errors.OfType<AssetErrorInfo>().ToList<AssetErrorInfo>()));
		}

		/// <summary>
		/// To save the Asset Fuel Burn rate settings
		/// </summary>
		/// <param name="burnRateRequest"></param>
		/// <returns></returns>
		[HttpPut]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(AssetFuelBurnRateSettingsResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(AssetFuelBurnRateSettingsResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(AssetFuelBurnRateSettingsResponse), (int)HttpStatusCode.InternalServerError)]
		public async Task<ActionResult<AssetFuelBurnRateSettingsResponse>> Save(AssetFuelBurnRateSettingRequest burnRateRequest)
		{
			burnRateRequest = await base.ReadRequestContentAsync(burnRateRequest);

			burnRateRequest.CustomerUid = base.GetCustomerContext(Request);
			burnRateRequest.UserUid = base.GetUserContext(Request);
			burnRateRequest.StartDate = DateTime.Parse(DateTime.UtcNow.ToShortDateString());
			burnRateRequest.TargetValues = new Dictionary<AssetTargetType, double?>
				{
					{ AssetTargetType.IdlingBurnRateinLiPerHour, burnRateRequest.IdleTargetValue },
					{ AssetTargetType.WorkingBurnRateinLiPerHour, burnRateRequest.WorkTargetValue }
				};

			this._loggingService.Debug("Updating Asset Settings Burn Rate for the given request : " + JsonConvert.SerializeObject(burnRateRequest), MethodInfo.GetCurrentMethod().Name);

			var result = await this._assetSettingsService.Save(burnRateRequest);
			this._loggingService.Debug("Updating Asset Settings Burn Rate response : " + JsonConvert.SerializeObject(result), MethodInfo.GetCurrentMethod().Name);

			return base.SendResponse(HttpStatusCode.OK, new AssetFuelBurnRateSettingsResponse(result.AssetSettingsLists, result.Errors.OfType<AssetErrorInfo>().ToList<AssetErrorInfo>()));
		}
	}
}
