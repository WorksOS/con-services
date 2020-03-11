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
using ClientModel.DeviceConfig.Request.DeviceConfig.ReportingSchedule;
using ClientModel.DeviceConfig.Response.DeviceConfig.ReportingSchedule;
using CommonApiLibrary.Filters;
using ClientModel.DeviceConfig.Response.DeviceConfig.MaintenanceMode;
using ClientModel.DeviceConfig.Request.DeviceConfig.MaintenanceMode;
using System.Linq;

namespace DeviceSettings.Controller
{
	[Route("v1/deviceconfigs/maintenancemode")]
	public class DeviceConfigMaintenanceModeController : DeviceConfigApiControllerBase
	{
		private readonly IDeviceConfigService<DeviceConfigMaintenanceModeRequest, DeviceConfigMaintenanceModeDetails> _maintenanceModeService;
		private readonly string _parameterGroupName = "MaintenanceMode";

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reportingScheduleService"></param>
		/// <param name="injectConfig"></param>
		/// <param name="loggingService"></param>
		public DeviceConfigMaintenanceModeController(IDeviceConfigService<DeviceConfigMaintenanceModeRequest, DeviceConfigMaintenanceModeDetails> MaintenanceModeService,
			IInjectConfig injectConfig, ILoggingService loggingService) : base(injectConfig.ResolveKeyed<DeviceConfigRequestToAttributeMaps>("DeviceConfigRequestToAttributeMaps"), loggingService)
		{
			base._loggingService.CreateLogger(typeof(DeviceConfigMaintenanceModeController));
			this._maintenanceModeService = MaintenanceModeService;
		}

		[HttpPost]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(DeviceConfigMaintenanceModeResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(DeviceConfigMaintenanceModeResponse), (int) HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(DeviceConfigMaintenanceModeResponse), (int)HttpStatusCode.InternalServerError)]
		public async Task<ActionResult<DeviceConfigMaintenanceModeResponse>> Fetch(DeviceConfigRequestBase deviceConfigBaseRequest)
		{
			this._loggingService.Info("Fetching Maintenance Mode with request : " + JsonConvert.SerializeObject(deviceConfigBaseRequest), "DeviceConfigMaintenanceModeController.Fetch");

			deviceConfigBaseRequest = await base.ReadRequestContentAsync(deviceConfigBaseRequest);

			DeviceConfigMaintenanceModeRequest deviceConfigMaintenanceModeRequest = new DeviceConfigMaintenanceModeRequest();
			if (deviceConfigBaseRequest == null)
			{
				throw new ArgumentNullException("deviceConfigBaseRequest is null");
			}
			else
			{
				deviceConfigMaintenanceModeRequest.AssetUIDs = deviceConfigBaseRequest.AssetUIDs;
			}

			deviceConfigMaintenanceModeRequest.ParameterGroupName = _parameterGroupName;
			deviceConfigMaintenanceModeRequest.DeviceType = deviceConfigBaseRequest.DeviceType;
			deviceConfigMaintenanceModeRequest.UserUID = base.GetUserContext(Request);
			deviceConfigMaintenanceModeRequest.CustomerUID = base.GetCustomerContext(Request);

			deviceConfigMaintenanceModeRequest.ConfigValues = new Dictionary<string, string>
				{
					{this._attributeMaps.Values["Status"], string.Empty},
					{this._attributeMaps.Values["StartTime"], string.Empty},
					{this._attributeMaps.Values["MaintenanceModeDuration"], string.Empty}
				};

			this._loggingService.Info("Started invoking MaintenanceModeService with request : " + JsonConvert.SerializeObject(deviceConfigBaseRequest), "DeviceConfigMaintenanceModeController.Fetch");

			var response = await this._maintenanceModeService.Fetch(deviceConfigMaintenanceModeRequest);

			this._loggingService.Info("Ended invoking MaintenanceModeService with response : " + JsonConvert.SerializeObject(response), "DeviceConfigMaintenanceModeController.Fetch");

			return base.SendResponse(HttpStatusCode.OK, new DeviceConfigMaintenanceModeResponse(response.Lists, response.Errors.OfType<AssetErrorInfo>().ToList()));

		}

		[HttpPut]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(DeviceConfigMaintenanceModeResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(DeviceConfigMaintenanceModeResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(DeviceConfigMaintenanceModeResponse), (int)HttpStatusCode.InternalServerError)]
		public async Task<ActionResult<DeviceConfigMaintenanceModeResponse>> Save(DeviceConfigMaintenanceModeRequest deviceConfigMaintenanceModeRequest)
		{
			this._loggingService.Info("Saving Maintenance Mode with request : " + JsonConvert.SerializeObject(deviceConfigMaintenanceModeRequest), "DeviceConfigMaintenanceModeController.Save");

			deviceConfigMaintenanceModeRequest = await base.ReadRequestContentAsync(deviceConfigMaintenanceModeRequest);

			if (deviceConfigMaintenanceModeRequest == null)
			{
				throw new ArgumentNullException("deviceConfigMaintenanceModeRequest is null");
			}

			deviceConfigMaintenanceModeRequest.ParameterGroupName = _parameterGroupName;
			deviceConfigMaintenanceModeRequest.UserUID = base.GetUserContext(Request);
			deviceConfigMaintenanceModeRequest.CustomerUID = base.GetCustomerContext(Request);
			deviceConfigMaintenanceModeRequest.StartTime = DateTime.UtcNow;
			deviceConfigMaintenanceModeRequest.ConfigValues = new Dictionary<string, string>();

			if (deviceConfigMaintenanceModeRequest.Status.HasValue)
			{
				deviceConfigMaintenanceModeRequest.ConfigValues.Add(this._attributeMaps.Values["Status"], deviceConfigMaintenanceModeRequest.Status.ToString());

				if (deviceConfigMaintenanceModeRequest.Status.Value)
				{
					deviceConfigMaintenanceModeRequest.ConfigValues.Add(this._attributeMaps.Values["StartTime"], deviceConfigMaintenanceModeRequest.StartTime.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));

					if (deviceConfigMaintenanceModeRequest.MaintenanceModeDuration.HasValue)
					{
						deviceConfigMaintenanceModeRequest.ConfigValues.Add(this._attributeMaps.Values["MaintenanceModeDuration"], deviceConfigMaintenanceModeRequest.MaintenanceModeDuration.ToString());
					}
				}
			}

			this._loggingService.Info("Started invoking MaintenanceModeService with request : " + JsonConvert.SerializeObject(deviceConfigMaintenanceModeRequest), "DeviceConfigMaintenanceModeController.Save");

			var response = await this._maintenanceModeService.Save(deviceConfigMaintenanceModeRequest);

			this._loggingService.Info("Ended invoking MaintenanceModeService with response : " + JsonConvert.SerializeObject(response), "DeviceConfigMaintenanceModeController.Save");

			return base.SendResponse(HttpStatusCode.OK, new DeviceConfigMaintenanceModeResponse(response.Lists, response.Errors.OfType<AssetErrorInfo>().ToList()));
		}
	}
}