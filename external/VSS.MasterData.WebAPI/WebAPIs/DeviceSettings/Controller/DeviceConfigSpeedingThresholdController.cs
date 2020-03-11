using CommonApiLibrary.Filters;
using CommonModel.Error;
using Infrastructure.Common.DeviceSettings.Helpers;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Infrastructure.Common.DeviceSettings.Enums;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Utilities.Logging;
using ClientModel.DeviceConfig.Request.DeviceConfig.SpeedingThresholds;
using ClientModel.DeviceConfig.Response.DeviceConfig.SpeedingThresholds;
using Utilities.IOC;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using CommonModel.DeviceSettings.ConfigNameValues;
using CommonModel.Exceptions;
using System.Linq;

namespace DeviceSettings.Controller
{
	[Route("v1/deviceconfigs/speedingthreshold")]
	public class DeviceConfigSpeedingThresholdController : DeviceConfigApiControllerBase
	{
		private readonly IDeviceConfigService<DeviceConfigSpeedingThresholdsRequest, DeviceConfigSpeedingThresholdsDetails> _speedingThresholdsService;
		private readonly string ParameterGroupName = "SpeedingThresholds";

		/// <summary>
		/// 
		/// </summary>
		/// <param name="SpeedingThresholdsService"></param>
		/// <param name="injectConfig"></param>
		/// <param name="loggingService"></param>
		public DeviceConfigSpeedingThresholdController(IDeviceConfigService<DeviceConfigSpeedingThresholdsRequest, DeviceConfigSpeedingThresholdsDetails> speedingThresholdsService,
			IInjectConfig injectConfig, ILoggingService loggingService) : base(injectConfig.ResolveKeyed<DeviceConfigRequestToAttributeMaps>("DeviceConfigRequestToAttributeMaps"), loggingService)
		{
			base._loggingService.CreateLogger(typeof(DeviceConfigSpeedingThresholdController));
			this._speedingThresholdsService = speedingThresholdsService;
		}

		[HttpPost]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(DeviceConfigSpeedingThresholdsResponse),(int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(DeviceConfigSpeedingThresholdsResponse),(int)HttpStatusCode.BadRequest )]
		[ProducesResponseType(typeof(DeviceConfigSpeedingThresholdsResponse),(int)HttpStatusCode.InternalServerError )]
		public async Task<ActionResult<DeviceConfigSpeedingThresholdsResponse>> Fetch(DeviceConfigRequestBase deviceConfigBaseRequest)
		{

			this._loggingService.Info("Fetching Speeding Thresholds for request : " + JsonConvert.SerializeObject(deviceConfigBaseRequest), "DeviceConfigSpeedingThresholdsController.Fetch");
			DeviceConfigSpeedingThresholdsRequest deviceConfigSpeedingThresholdsRequest = new DeviceConfigSpeedingThresholdsRequest();

			deviceConfigBaseRequest = await base.ReadRequestContentAsync(deviceConfigBaseRequest);

			if (deviceConfigBaseRequest == null)
			{
				throw new ArgumentNullException("deviceConfigBaseRequest is null");
			}
			else
			{
				deviceConfigSpeedingThresholdsRequest.AssetUIDs = deviceConfigBaseRequest.AssetUIDs;
			}

			deviceConfigSpeedingThresholdsRequest.ParameterGroupName = ParameterGroupName;
			deviceConfigSpeedingThresholdsRequest.DeviceType = deviceConfigBaseRequest.DeviceType;
			deviceConfigSpeedingThresholdsRequest.UserUID = base.GetUserContext(Request);
			deviceConfigSpeedingThresholdsRequest.CustomerUID = base.GetCustomerContext(Request);
			deviceConfigSpeedingThresholdsRequest.ConfigValues = new Dictionary<string, string>
				{
					{this._attributeMaps.Values["SpeedThreshold"], string.Empty},
					{this._attributeMaps.Values["SpeedThresholdEnabled"], string.Empty},
					{this._attributeMaps.Values["SpeedThresholdDuration"], string.Empty}
				};

			this._loggingService.Info("Started invoking SpeedingThresholdsService with request : " + JsonConvert.SerializeObject(deviceConfigSpeedingThresholdsRequest), "DeviceConfigSpeedingThresholdController.Fetch");

			var response = await this._speedingThresholdsService.Fetch(deviceConfigSpeedingThresholdsRequest);

			this._loggingService.Info("Ended invoking SpeedingThresholdsService with response : " + JsonConvert.SerializeObject(response), "DeviceConfigSpeedingThresholdController.Fetch");

			return base.SendResponse(HttpStatusCode.OK, new DeviceConfigSpeedingThresholdsResponse(response.Lists, response.Errors.OfType<AssetErrorInfo>().ToList()));
		}

		[HttpPut]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(DeviceConfigSpeedingThresholdsResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(DeviceConfigSpeedingThresholdsResponse), (int)HttpStatusCode.BadRequest )]
		[ProducesResponseType(typeof(DeviceConfigSpeedingThresholdsResponse), (int)HttpStatusCode.InternalServerError)]
		public async Task<ActionResult<DeviceConfigSpeedingThresholdsResponse>> Save(DeviceConfigSpeedingThresholdsRequest deviceConfigSpeedingThresholdsRequest)
		{

			this._loggingService.Info("Saving Speeding Thresholds for request : " + JsonConvert.SerializeObject(deviceConfigSpeedingThresholdsRequest), "DeviceConfigSpeedingThresholdsController.Save");

			deviceConfigSpeedingThresholdsRequest = await base.ReadRequestContentAsync(deviceConfigSpeedingThresholdsRequest);

			if (deviceConfigSpeedingThresholdsRequest == null)
			{
				throw new ArgumentNullException("deviceConfigSpeedingThresholdsRequest is null");
			}

			deviceConfigSpeedingThresholdsRequest.ParameterGroupName = ParameterGroupName;
			deviceConfigSpeedingThresholdsRequest.UserUID = base.GetUserContext(Request);
			deviceConfigSpeedingThresholdsRequest.CustomerUID = base.GetCustomerContext(Request);

			deviceConfigSpeedingThresholdsRequest.ConfigValues = new Dictionary<string, string>();

			if (deviceConfigSpeedingThresholdsRequest.SpeedThresholdEnabled.HasValue)
			{
				deviceConfigSpeedingThresholdsRequest.ConfigValues.Add(this._attributeMaps.Values["SpeedThresholdEnabled"], deviceConfigSpeedingThresholdsRequest.SpeedThresholdEnabled.ToString());

				if (deviceConfigSpeedingThresholdsRequest.SpeedThresholdEnabled.Value)
				{
					if (deviceConfigSpeedingThresholdsRequest.SpeedThresholdDuration.HasValue)
					{
						deviceConfigSpeedingThresholdsRequest.ConfigValues.Add(this._attributeMaps.Values["SpeedThresholdDuration"], deviceConfigSpeedingThresholdsRequest.SpeedThresholdDuration.ToString());
					}
					if (deviceConfigSpeedingThresholdsRequest.SpeedThreshold.HasValue)
					{
						deviceConfigSpeedingThresholdsRequest.ConfigValues.Add(this._attributeMaps.Values["SpeedThreshold"], deviceConfigSpeedingThresholdsRequest.SpeedThreshold.ToString());
					}
				}
			}

			this._loggingService.Info("Started invoking SpeedingThresholdsService with request : " + JsonConvert.SerializeObject(deviceConfigSpeedingThresholdsRequest), "DeviceConfigSpeedingThresholdController.Save");

			var response = await this._speedingThresholdsService.Save(deviceConfigSpeedingThresholdsRequest);

			this._loggingService.Info("Ended invoking SpeedingThresholdsService with response : " + JsonConvert.SerializeObject(response), "DeviceConfigSpeedingThresholdController.Save");

			return base.SendResponse(HttpStatusCode.OK, new DeviceConfigSpeedingThresholdsResponse(response.Lists, response.Errors.OfType<AssetErrorInfo>().ToList()));

		}

	}
}