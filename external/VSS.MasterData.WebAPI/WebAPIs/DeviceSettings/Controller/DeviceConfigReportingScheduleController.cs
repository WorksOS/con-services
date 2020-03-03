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
using System.Linq;

namespace DeviceSettings.Controller
{
	[Route("v1/deviceconfigs/reportingschedule")]
	public class DeviceConfigReportingScheduleController : DeviceConfigApiControllerBase
	{

		private readonly IDeviceConfigService<DeviceConfigReportingScheduleRequest, DeviceConfigReportingScheduleDetails> _reportingScheduleService;
		private string _parameterGroupName = "ReportingSchedule";


		/// <summary>
		/// 
		/// </summary>
		/// <param name="reportingScheduleService"></param>
		/// <param name="injectConfig"></param>
		/// <param name="loggingService"></param>
		public DeviceConfigReportingScheduleController(IDeviceConfigService<DeviceConfigReportingScheduleRequest, DeviceConfigReportingScheduleDetails> reportingScheduleService,
			IInjectConfig injectConfig, ILoggingService loggingService) : base(injectConfig.ResolveKeyed<DeviceConfigRequestToAttributeMaps>("DeviceConfigRequestToAttributeMaps"), loggingService)
		{
			base._loggingService.CreateLogger(typeof(DeviceConfigReportingScheduleController));
			this._reportingScheduleService = reportingScheduleService;
		}

		/// <summary>
		/// To Fetch the Device Config Reporting Schedule values.
		/// </summary>
		/// <param name="deviceConfigBaseRequest">Device Config Base Request</param>
		/// <returns></returns>
		[HttpPost]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(DeviceConfigReportingScheduleResponse), (int) HttpStatusCode.OK )]
		[ProducesResponseType(typeof(DeviceConfigReportingScheduleResponse), (int) HttpStatusCode.BadRequest  )]
		[ProducesResponseType(typeof(DeviceConfigReportingScheduleResponse), (int) HttpStatusCode.InternalServerError)]
		public async Task<ActionResult<DeviceConfigReportingScheduleResponse>> Fetch(DeviceConfigRequestBase deviceConfigBaseRequest)
		{
			this._loggingService.Info("Fetching Reporting Schedule for request : " + JsonConvert.SerializeObject(deviceConfigBaseRequest), "DeviceConfigReportingScheduleController.Fetch");

			deviceConfigBaseRequest = await base.ReadRequestContentAsync(deviceConfigBaseRequest);

			DeviceConfigReportingScheduleRequest deviceConfigReportingScheduleRequest = new DeviceConfigReportingScheduleRequest();

			if (deviceConfigBaseRequest == null)
			{
				throw new ArgumentNullException("deviceConfigBaseRequest is null");
			}
			else
			{
				deviceConfigReportingScheduleRequest.AssetUIDs = deviceConfigBaseRequest.AssetUIDs;
			}

			deviceConfigReportingScheduleRequest.ParameterGroupName = _parameterGroupName;
			deviceConfigReportingScheduleRequest.DeviceType = deviceConfigBaseRequest.DeviceType;
			deviceConfigReportingScheduleRequest.UserUID = base.GetUserContext(Request);
			deviceConfigReportingScheduleRequest.CustomerUID = base.GetCustomerContext(Request);

			deviceConfigReportingScheduleRequest.ConfigValues = new Dictionary<string, string>
				{
					{this._attributeMaps.Values["DailyReportingTime"], string.Empty},
					{this._attributeMaps.Values["DailyLocationReportingFrequency"], string.Empty},
					{this._attributeMaps.Values["HourMeterFuelReport"], string.Empty},
					{this._attributeMaps.Values["ReportAssetStartStop"], string.Empty},
					{this._attributeMaps.Values["GlobalGram"], string.Empty}
				};

			this._loggingService.Info("Started invoking ReportingScheduleService with request : " + JsonConvert.SerializeObject(deviceConfigBaseRequest), "DeviceConfigReportingScheduleController.Fetch");

			var response = await this._reportingScheduleService.Fetch(deviceConfigReportingScheduleRequest);

			this._loggingService.Info("Ended invoking ReportingScheduleService with response : " + JsonConvert.SerializeObject(response), "DeviceConfigReportingScheduleController.Fetch");

			return base.SendResponse(HttpStatusCode.OK, new DeviceConfigReportingScheduleResponse(response.Lists, response.Errors.OfType<AssetErrorInfo>().ToList()));

		}

		/// <summary>
		/// To Save the Device Config Reporting Schedule values.
		/// </summary>
		/// <param name="deviceConfigReportingScheduleRequest">deviceConfigReportingScheduleRequest</param>
		/// <returns>DeviceConfigReportingScheduleResponse</returns>
		[HttpPut]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(DeviceConfigReportingScheduleResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(DeviceConfigReportingScheduleResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(DeviceConfigReportingScheduleResponse), (int)HttpStatusCode.InternalServerError)]
		public async Task<ActionResult<DeviceConfigReportingScheduleResponse>> Save(DeviceConfigReportingScheduleRequest deviceConfigReportingScheduleRequest)
		{
			this._loggingService.Info("Saving Reporting Schedule for request : " + JsonConvert.SerializeObject(deviceConfigReportingScheduleRequest), "DeviceConfigReportingScheduleController.Save");

			deviceConfigReportingScheduleRequest = await base.ReadRequestContentAsync(deviceConfigReportingScheduleRequest);

			if (deviceConfigReportingScheduleRequest == null)
			{
				throw new ArgumentNullException("deviceConfigReportingScheduleRequest is null");
			}

			deviceConfigReportingScheduleRequest.ParameterGroupName = _parameterGroupName;
			deviceConfigReportingScheduleRequest.UserUID = base.GetUserContext(Request);
			deviceConfigReportingScheduleRequest.CustomerUID = base.GetCustomerContext(Request);

			deviceConfigReportingScheduleRequest.ConfigValues = new Dictionary<string, string>();

			if (deviceConfigReportingScheduleRequest.DailyReportingTime.HasValue)
			{
				deviceConfigReportingScheduleRequest.ConfigValues.Add(this._attributeMaps.Values["DailyReportingTime"], deviceConfigReportingScheduleRequest.DailyReportingTime.ToString());
			}

			if (deviceConfigReportingScheduleRequest.DailyLocationReportingFrequency.HasValue)
			{
				deviceConfigReportingScheduleRequest.ConfigValues.Add(this._attributeMaps.Values["DailyLocationReportingFrequency"], ((int)deviceConfigReportingScheduleRequest.DailyLocationReportingFrequency).ToString());
			}

			if (deviceConfigReportingScheduleRequest.HourMeterFuelReport.HasValue)
			{
				deviceConfigReportingScheduleRequest.ConfigValues.Add(this._attributeMaps.Values["HourMeterFuelReport"], deviceConfigReportingScheduleRequest.HourMeterFuelReport.ToString());
			}

			if (deviceConfigReportingScheduleRequest.ReportAssetStartStop.HasValue)
			{
				deviceConfigReportingScheduleRequest.ConfigValues.Add(this._attributeMaps.Values["ReportAssetStartStop"], deviceConfigReportingScheduleRequest.ReportAssetStartStop.ToString());
			}

			if (deviceConfigReportingScheduleRequest.GlobalGram.HasValue)
			{
				deviceConfigReportingScheduleRequest.ConfigValues.Add(this._attributeMaps.Values["GlobalGram"], deviceConfigReportingScheduleRequest.GlobalGram.ToString());
			}

			this._loggingService.Info("Started invoking ReportingScheduleService with request : " + JsonConvert.SerializeObject(deviceConfigReportingScheduleRequest), "DeviceConfigReportingScheduleController.Save");

			var response = await this._reportingScheduleService.Save(deviceConfigReportingScheduleRequest);

			this._loggingService.Info("Ended invoking ReportingScheduleService with response : " + JsonConvert.SerializeObject(response), "DeviceConfigReportingScheduleController.Save");

			return base.SendResponse(HttpStatusCode.OK, new DeviceConfigReportingScheduleResponse(response.Lists, response.Errors.OfType<AssetErrorInfo>().ToList()));
		}
	}
}

