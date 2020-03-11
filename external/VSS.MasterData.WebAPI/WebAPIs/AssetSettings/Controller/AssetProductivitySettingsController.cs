using AutoMapper;
using ClientModel.AssetSettings.Request;
using ClientModel.AssetSettings.Response.ProductivityTargetsResponse;
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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Utilities.Logging;

namespace AssetSettings.Controller
{
	public class AssetProductivitySettingsController : ApiControllerBase
	{

		private AssetWeeklySettingsService _productivityTargetsService;
		private readonly ILoggingService _loggingService;
		private readonly IMapper _mapper;
		private readonly IAssetSettingsPublisher _publisher;
		private IAssetSettingsTypeHandler<AssetSettingsBase> _converter;
		public AssetProductivitySettingsController(Func<string, AssetWeeklySettingsService> weeklySettingsService, IAssetSettingsPublisher publisher, IAssetSettingsTypeHandler<AssetSettingsBase> converter, ILoggingService loggingService, IMapper mapper)
		{
			_mapper = mapper;
			_productivityTargetsService = weeklySettingsService("ProductivityTargets");
			_publisher = publisher;
			_converter = converter;
			_loggingService = loggingService;
			this._loggingService.CreateLogger(this.GetType());
		}

		[HttpPost]
		[Route("v1/assetproductivitysettings/")]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(GetProductivityTargetsResponse))]
		[ProducesResponseType((int)HttpStatusCode.NoContent, Type = typeof(GetProductivityTargetsResponse))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(GetProductivityTargetsResponse))]
		[ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(GetProductivityTargetsResponse))]
		// POST api/<controller>
		public async Task<ActionResult<GetProductivityTargetsResponse>> PostProductivityTargets([FromQuery]DateTime? startDate = null, [FromQuery]DateTime? endDate = null, string[] assetIdentifiers = null)
		{
			string jsonContent = await base.ReadRequestContentAsStringAsync();
			assetIdentifiers = JsonConvert.DeserializeObject<string[]>(jsonContent);
			var targetHours = await _productivityTargetsService.GetAssetSettings(assetIdentifiers, startDate, endDate);
			//_mapper.CreateMap(typeof(GetAssetWeeklyTargetsResponse), typeof(GetProductivityTargetsResponse));
			var productivityTargets = _mapper.Map<GetProductivityTargetsResponse>(targetHours);
			if (productivityTargets != null && productivityTargets.Errors != null && productivityTargets.Errors.Any())
			{
				return base.SendResponse(HttpStatusCode.BadRequest, productivityTargets);
			}
			return base.SendResponse(HttpStatusCode.OK, productivityTargets);
		}

		// PUT api/<controller>/5
		[HttpPut]
		[Route("v1/assetproductivitysettings/")]
		[UserUidParser]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(EditProductivityTargetsResponse))]
		[ProducesResponseType((int)HttpStatusCode.NoContent, Type = typeof(EditProductivityTargetsResponse))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(EditProductivityTargetsResponse))]
		[ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(EditProductivityTargetsResponse))]
		//[ChunkedEncodingFilter(typeof(AssetProductivityTargets), "assetProductivitySettings")]
		public async Task<ActionResult<EditProductivityTargetsResponse>> Put(AssetProductivityTargets assetProductivityParameter)
		{
			assetProductivityParameter = await base.ReadRequestContentAsync(assetProductivityParameter);

			assetProductivityParameter.UserUID = base.GetUserContext(Request);
			assetProductivityParameter.CustomerUID = base.GetCustomerContext(Request);
			_loggingService.Debug(string.Format("UserUID : {0}, CustomerUID : {1}", assetProductivityParameter.UserUID, assetProductivityParameter.CustomerUID), "AssetSettingsPublisher.publishAssetWeeklySettings");
			_loggingService.Debug(string.Format("Given Request : {0}", JsonConvert.SerializeObject(assetProductivityParameter)), "AssetSettingsPublisher.publishAssetWeeklySettings");

			var editResponse = await _productivityTargetsService.EditAssetSettings(assetProductivityParameter.assetProductivitySettings.ToArray());
			var userAssetWeeklySettings = new List<AssetSettingsGetDBResponse>();
			assetProductivityParameter.assetProductivitySettings.ForEach(target => userAssetWeeklySettings.AddRange(_converter.GetCommonResponseFromProductivityTargetsAndAssetTargets(target)));
			_publisher.PublishUserWeeklyAssetSettings(userAssetWeeklySettings, assetProductivityParameter.UserUID.Value, assetProductivityParameter.CustomerUID.Value);

			if (editResponse != null && editResponse.Errors != null && editResponse.Errors.Any())
			{
				return base.SendResponse(HttpStatusCode.BadRequest, editResponse);
			}

			if (editResponse == null)
			{
				return base.SendResponse<EditProductivityTargetsResponse>(HttpStatusCode.NoContent, null);
			}
			return base.SendResponse(HttpStatusCode.OK, editResponse);
		}
	}
}
