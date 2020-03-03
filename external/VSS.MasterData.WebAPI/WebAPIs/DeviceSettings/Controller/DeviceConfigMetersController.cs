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
using Utilities.IOC;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using CommonModel.DeviceSettings.ConfigNameValues;
using CommonModel.Exceptions;
using CommonApiLibrary.Filters;
using ClientModel.DeviceConfig.Response.DeviceConfig.Meters;
using ClientModel.DeviceConfig.Request.DeviceConfig.Meters;
using System.Linq;

namespace DeviceSettings.Controller
{
	[Route("v1/deviceconfigs/meters")]
	public class DeviceConfigMetersController : DeviceConfigApiControllerBase
	{
		private readonly IDeviceConfigService<DeviceConfigMetersRequest, DeviceConfigMetersDetails> _metersService;
		private readonly string ParameterGroupName = "Meters";

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reportingScheduleService"></param>
		/// <param name="injectConfig"></param>
		/// <param name="loggingService"></param>
		public DeviceConfigMetersController(IDeviceConfigService<DeviceConfigMetersRequest, DeviceConfigMetersDetails> metersService,
			IInjectConfig injectConfig, ILoggingService loggingService) : base(injectConfig.ResolveKeyed<DeviceConfigRequestToAttributeMaps>("DeviceConfigRequestToAttributeMaps"), loggingService)
		{
			base._loggingService.CreateLogger(typeof(DeviceConfigMetersController));
			this._metersService = metersService;
		}

		[HttpPost]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(DeviceConfigMetersResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(DeviceConfigMetersResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(DeviceConfigMetersResponse), (int)HttpStatusCode.InternalServerError)]
		public async Task<ActionResult<DeviceConfigMetersResponse>> Fetch(DeviceConfigRequestBase deviceConfigBaseRequest)
		{
			this._loggingService.Info("Fetching Meters for request : " + JsonConvert.SerializeObject(deviceConfigBaseRequest), "DeviceConfigMetersController.Fetch");

			DeviceConfigMetersRequest deviceConfigMetersRequest = new DeviceConfigMetersRequest();

			deviceConfigBaseRequest = await base.ReadRequestContentAsync(deviceConfigBaseRequest);

			if (deviceConfigBaseRequest == null)
			{
				throw new ArgumentNullException("deviceConfigBaseRequest is null");
			}
			else
			{
				deviceConfigMetersRequest.AssetUIDs = deviceConfigBaseRequest.AssetUIDs;
			}

			deviceConfigMetersRequest.ParameterGroupName = ParameterGroupName;
			deviceConfigMetersRequest.DeviceType = deviceConfigBaseRequest.DeviceType;
			deviceConfigMetersRequest.UserUID = base.GetUserContext(Request);
			deviceConfigMetersRequest.CustomerUID = base.GetCustomerContext(Request);

			deviceConfigMetersRequest.ConfigValues = new Dictionary<string, string>
				{
					{this._attributeMaps.Values["SmhOdometerConfig"], string.Empty},
					{this._attributeMaps.Values["HoursMeterProposedValue"], string.Empty},
					{this._attributeMaps.Values["HoursMeterCurrentValue"], string.Empty},
					{this._attributeMaps.Values["OdoMeterProposedValue"], string.Empty},
					{this._attributeMaps.Values["OdoMeterCurrentValue"], string.Empty}
				};

			this._loggingService.Info("Started invoking MetersService with request : " + JsonConvert.SerializeObject(deviceConfigMetersRequest), "DeviceConfigMetersController.Fetch");

			var response = await this._metersService.Fetch(deviceConfigMetersRequest);

			this._loggingService.Info("Ended invoking MetersService with response : " + JsonConvert.SerializeObject(response), "DeviceConfigMetersController.Fetch");

			return base.SendResponse(HttpStatusCode.OK, new DeviceConfigMetersResponse(response.Lists, response.Errors.OfType<AssetErrorInfo>().ToList()));
		}

		[HttpPut]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(DeviceConfigMetersResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(DeviceConfigMetersResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(DeviceConfigMetersResponse), (int)HttpStatusCode.InternalServerError)]
		public async Task<ActionResult<DeviceConfigMetersResponse>> Save(DeviceConfigMetersRequest deviceConfigMetersRequest)
		{
			this._loggingService.Info("Saving Meters for request : " + JsonConvert.SerializeObject(deviceConfigMetersRequest), "DeviceConfigMetersController.Save");

			deviceConfigMetersRequest = await base.ReadRequestContentAsync(deviceConfigMetersRequest);

			if (deviceConfigMetersRequest == null)
			{
				throw new ArgumentNullException("deviceConfigMetersRequest is null");
			}

			deviceConfigMetersRequest.ParameterGroupName = ParameterGroupName;
			deviceConfigMetersRequest.UserUID = base.GetUserContext(Request);
			deviceConfigMetersRequest.CustomerUID = base.GetCustomerContext(Request);

			deviceConfigMetersRequest.ConfigValues = new Dictionary<string, string>();

			if (deviceConfigMetersRequest.SmhOdometerConfig.HasValue)
			{
				deviceConfigMetersRequest.ConfigValues.Add(this._attributeMaps.Values["SmhOdometerConfig"], deviceConfigMetersRequest.SmhOdometerConfig.ToString());
			}
			if (deviceConfigMetersRequest.HoursMeter != null && deviceConfigMetersRequest.HoursMeter.ProposedValue.HasValue)
			{
				deviceConfigMetersRequest.ConfigValues.Add(this._attributeMaps.Values["HoursMeterProposedValue"], deviceConfigMetersRequest.HoursMeter.ProposedValue.ToString());
			}
			if (deviceConfigMetersRequest.HoursMeter != null && deviceConfigMetersRequest.HoursMeter.CurrentValue.HasValue)
			{
				deviceConfigMetersRequest.ConfigValues.Add(this._attributeMaps.Values["HoursMeterCurrentValue"], deviceConfigMetersRequest.HoursMeter.CurrentValue.ToString());
			}
			if (deviceConfigMetersRequest.OdoMeter != null && deviceConfigMetersRequest.OdoMeter.ProposedValue.HasValue)
			{
				deviceConfigMetersRequest.ConfigValues.Add(this._attributeMaps.Values["OdoMeterProposedValue"], deviceConfigMetersRequest.OdoMeter.ProposedValue.ToString());
			}
			if (deviceConfigMetersRequest.OdoMeter != null && deviceConfigMetersRequest.OdoMeter.CurrentValue.HasValue)
			{
				deviceConfigMetersRequest.ConfigValues.Add(this._attributeMaps.Values["OdoMeterCurrentValue"], deviceConfigMetersRequest.OdoMeter.CurrentValue.ToString());
			}

			this._loggingService.Info("Started invoking MetersService with request : " + JsonConvert.SerializeObject(deviceConfigMetersRequest), "DeviceConfigMetersController.Save");

			var response = await this._metersService.Save(deviceConfigMetersRequest);

			this._loggingService.Info("Ended invoking MetersService with response : " + JsonConvert.SerializeObject(response), "DeviceConfigMetersController.Save");

			return base.SendResponse(HttpStatusCode.OK, new DeviceConfigMetersResponse(response.Lists, response.Errors.OfType<AssetErrorInfo>().ToList()));
		}
	}
}