using CommonModel.Error;
using Infrastructure.Common.DeviceSettings.Helpers;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Infrastructure.Common.DeviceSettings.Enums;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Utilities.Logging;
using Utilities.IOC;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using CommonModel.DeviceSettings.ConfigNameValues;
using CommonModel.Exceptions;
using CommonApiLibrary.Filters;
using ClientModel.DeviceConfig.Request.DeviceConfig.MovingThresold;
using ClientModel.DeviceConfig.Response.DeviceConfig.MovingThreshold;

namespace DeviceSettings.Controller
{
	[Route("v1/deviceconfigs/movingthresholds")]
	public class DeviceConfigMovingThresholdController : DeviceConfigApiControllerBase
	{ 
		 private readonly IDeviceConfigService<DeviceConfigMovingThresholdRequest, DeviceConfigMovingThresholdDetails> _movingThresholdService;


		public DeviceConfigMovingThresholdController(IDeviceConfigService<DeviceConfigMovingThresholdRequest, DeviceConfigMovingThresholdDetails> movingThresholdService,
			IInjectConfig injectConfig, ILoggingService loggingService) : base(injectConfig.ResolveKeyed<DeviceConfigRequestToAttributeMaps>("DeviceConfigRequestToAttributeMaps"), loggingService)
		{
			base._loggingService.CreateLogger(typeof(DeviceConfigMovingThresholdController));
			this._movingThresholdService = movingThresholdService;
		}

		[HttpPost]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType( typeof(DeviceConfigMovingThresholdResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType( typeof(DeviceConfigMovingThresholdResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType( typeof(DeviceConfigMovingThresholdResponse), (int)HttpStatusCode.InternalServerError)]
		public async Task<ActionResult<DeviceConfigMovingThresholdResponse>> Fetch(DeviceConfigRequestBase deviceConfigBaseRequest)
		{
			this._loggingService.Info("Fetching Moving Thresholds for request : " + JsonConvert.SerializeObject(deviceConfigBaseRequest), "DeviceConfigMovingThresholdController.Fetch");

			deviceConfigBaseRequest = await base.ReadRequestContentAsync(deviceConfigBaseRequest);

			DeviceConfigMovingThresholdRequest deviceConfigMovingThresholdRequest = new DeviceConfigMovingThresholdRequest();
			if (deviceConfigMovingThresholdRequest == null)
			{
				throw new ArgumentNullException("deviceConfigBaseRequest is null");
			}
			else
			{
				deviceConfigMovingThresholdRequest.AssetUIDs = deviceConfigBaseRequest.AssetUIDs;
			}

			deviceConfigMovingThresholdRequest.ParameterGroupName = "MovingThresholds";
			deviceConfigMovingThresholdRequest.DeviceType = deviceConfigBaseRequest.DeviceType;
			deviceConfigMovingThresholdRequest.UserUID = base.GetUserContext(Request);
			deviceConfigMovingThresholdRequest.CustomerUID = base.GetCustomerContext(Request);

			deviceConfigMovingThresholdRequest.ConfigValues = new Dictionary<string, string>
					{
						{this._attributeMaps.Values["Radius"], string.Empty},
						{this._attributeMaps.Values["MovingThresholdsDuration"], string.Empty},
						{this._attributeMaps.Values["MovingOrStoppedThreshold"], string.Empty}
					};

			this._loggingService.Info("Started invoking MovingThresholdService with request : " + JsonConvert.SerializeObject(deviceConfigBaseRequest), "DeviceConfigMovingThresholdController.Fetch");

			var response = await this._movingThresholdService.Fetch(deviceConfigMovingThresholdRequest);

			this._loggingService.Info("Ended invoking MovingThresholdService  with response : " + JsonConvert.SerializeObject(response), "DeviceConfigMovingThresholdController.Fetch");

			return base.SendResponse(HttpStatusCode.OK, new DeviceConfigMovingThresholdResponse(response.Lists, response.Errors.OfType<AssetErrorInfo>().ToList()));
		}

		[HttpPut]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType( typeof(DeviceConfigMovingThresholdResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(DeviceConfigMovingThresholdResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType( typeof(DeviceConfigMovingThresholdResponse), (int)HttpStatusCode.InternalServerError)]
		public async Task<ActionResult<DeviceConfigMovingThresholdResponse>> Save(DeviceConfigMovingThresholdRequest deviceConfigMovingThresholdRequest)
		{
			this._loggingService.Info("Saving Moving Thresholds for request : " + JsonConvert.SerializeObject(deviceConfigMovingThresholdRequest), "DeviceConfigMovingThresholdController.Save");

			deviceConfigMovingThresholdRequest = await base.ReadRequestContentAsync(deviceConfigMovingThresholdRequest);

			if (deviceConfigMovingThresholdRequest == null)
			{
				throw new ArgumentNullException("deviceConfigMovingThresholdRequest is null");
			}

			deviceConfigMovingThresholdRequest.ParameterGroupName = "MovingThresholds";
			deviceConfigMovingThresholdRequest.UserUID = base.GetUserContext(Request);
			deviceConfigMovingThresholdRequest.CustomerUID = base.GetCustomerContext(Request);

			deviceConfigMovingThresholdRequest.ConfigValues = new Dictionary<string, string>();

			if (deviceConfigMovingThresholdRequest.MovingOrStoppedThreshold.HasValue)
			{
				deviceConfigMovingThresholdRequest.ConfigValues.Add(this._attributeMaps.Values["MovingOrStoppedThreshold"], deviceConfigMovingThresholdRequest.MovingOrStoppedThreshold.ToString());
			}

			if (deviceConfigMovingThresholdRequest.MovingThresholdsDuration.HasValue)
			{
				deviceConfigMovingThresholdRequest.ConfigValues.Add(this._attributeMaps.Values["MovingThresholdsDuration"], deviceConfigMovingThresholdRequest.MovingThresholdsDuration.ToString());
			}

			if (deviceConfigMovingThresholdRequest.Radius.HasValue)
			{
				deviceConfigMovingThresholdRequest.ConfigValues.Add(this._attributeMaps.Values["Radius"], deviceConfigMovingThresholdRequest.Radius.ToString());
			}

			this._loggingService.Info("Started invoking MovingThresholdService with request : " + JsonConvert.SerializeObject(deviceConfigMovingThresholdRequest), "DeviceConfigMovingThresholdController.Save");

			var response = await this._movingThresholdService.Save(deviceConfigMovingThresholdRequest);

			this._loggingService.Info("Ended invoking MovingThresholdService with response : " + JsonConvert.SerializeObject(response), "DeviceConfigMovingThresholdController.Save");

			return base.SendResponse(HttpStatusCode.OK, new DeviceConfigMovingThresholdResponse(response.Lists, response.Errors.OfType<AssetErrorInfo>().ToList()));

		}
	}
}