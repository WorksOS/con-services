using ClientModel.AssetSettings.Request;
using ClientModel.AssetSettings.Response.AssetTargets;
using CommonApiLibrary;
using CommonApiLibrary.Filters;
using CommonModel.AssetSettings;
using DbModel.AssetSettings;
using Infrastructure.Service.AssetSettings.Interfaces;
using Infrastructure.Service.AssetSettings.Service;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Utilities.Logging;

namespace AssetSettings.Controller
{
	public class AssetTargetSettingsController : ApiControllerBase
	{
		private AssetWeeklySettingsService _targetHoursTemplate;
		private readonly ILoggingService _loggingService;
		private readonly IAssetSettingsPublisher _publisher;
		private IAssetSettingsTypeHandler<AssetSettingsBase> _converter;

		public AssetTargetSettingsController(Func<string, AssetWeeklySettingsService> weeklySettingsService, ILoggingService loggingService, IAssetSettingsPublisher publisher, IAssetSettingsTypeHandler<AssetSettingsBase> converter)
		{
			_targetHoursTemplate = weeklySettingsService("AssetSettings");
			this._loggingService = loggingService;
			_publisher = publisher;
			_converter = converter;
			this._loggingService.CreateLogger(this.GetType());
		}

		// GET api/<controller>
		[HttpPost]
		[Route("v1/assettargetsettings")]
		[UserUidParser]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(GetWeeklyAssetSettingsResponse))]
		[ProducesResponseType((int)HttpStatusCode.NoContent, Type = typeof(GetWeeklyAssetSettingsResponse))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(GetWeeklyAssetSettingsResponse))]
		[ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(GetWeeklyAssetSettingsResponse))]
		// POST api/<controller>
		public async Task<ActionResult<GetWeeklyAssetSettingsResponse>> PostAssetTarget([FromQuery]DateTime? startDate = null, [FromQuery]DateTime? endDate = null, List<String> assetUids = null)
		{

			string jsonContent = await base.ReadRequestContentAsStringAsync();
			var assetIdentifiers = JsonConvert.DeserializeObject<string[]>(jsonContent);
			var response = await _targetHoursTemplate.GetAssetSettings(assetIdentifiers, startDate, endDate);
			if (response != null && response.Errors != null)
			{
				return base.SendResponse(HttpStatusCode.BadRequest, response);
			}
			return base.SendResponse(HttpStatusCode.OK, response);

		}

		// PUT api/<controller>/5
		[HttpPut]
		[Route("v1/assettargetsettings")]
		[UserUidParser]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(EditAssetTargetResponse))]
		[ProducesResponseType((int)HttpStatusCode.NoContent, Type = typeof(EditAssetTargetResponse))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(EditAssetTargetResponse))]
		[ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(EditAssetTargetResponse))]
		//[ChunkedEncodingFilter(typeof(AssetTargetSettings), "assetTargetSettings")]
		public async Task<ActionResult<EditAssetTargetResponse>> Put(AssetTargetSettings assetTargetParameter)
		{
			assetTargetParameter = await base.ReadRequestContentAsync(assetTargetParameter);

			assetTargetParameter.UserUID = base.GetUserContext(Request);
			assetTargetParameter.CustomerUID = base.GetCustomerContext(Request);
			_loggingService.Debug(string.Format("Given Request : {0}", JsonConvert.SerializeObject(assetTargetParameter)), "AssetSettingsPublisher.publishAssetWeeklySettings");
			_loggingService.Debug(string.Format("UserUID : {0}, CustomerUID : {1}", assetTargetParameter.UserUID, assetTargetParameter.CustomerUID), "AssetSettingsPublisher.publishAssetWeeklySettings");

			var editResponse = await _targetHoursTemplate.EditAssetSettings(assetTargetParameter.assetTargetSettings.ToArray());
			var userAssetWeeklySettings = new List<AssetSettingsGetDBResponse>();
			assetTargetParameter.assetTargetSettings.ForEach(target => userAssetWeeklySettings.AddRange(_converter.GetCommonResponseFromProductivityTargetsAndAssetTargets(target)));
			_publisher.PublishUserWeeklyAssetSettings(userAssetWeeklySettings, assetTargetParameter.UserUID.Value, assetTargetParameter.CustomerUID.Value);
			if (editResponse != null && editResponse.Errors != null)
			{
				return base.SendResponse(HttpStatusCode.BadRequest, editResponse);
			}
			return base.SendResponse(HttpStatusCode.OK, editResponse);
		}
	}
}