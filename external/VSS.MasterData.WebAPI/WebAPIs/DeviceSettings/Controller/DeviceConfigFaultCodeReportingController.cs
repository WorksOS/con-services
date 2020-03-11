using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.FaultCodeReporting;
using ClientModel.DeviceConfig.Response.DeviceConfig.FaultCodeReporting;
using CommonApiLibrary.Filters;
using CommonModel.DeviceSettings.ConfigNameValues;
using CommonModel.Error;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Utilities.IOC;
using Utilities.Logging;

namespace DeviceSettings.Controller
{
	[Route("v1/deviceconfigs/faultcodereporting")]
	public class DeviceConfigFaultCodeReportingController : DeviceConfigApiControllerBase
	{
		private readonly IDeviceConfigService<DeviceConfigFaultCodeReportingRequest, DeviceConfigFaultCodeReportingDetails> _faultCodeReportingService;
		private readonly string ParameterGroupName = "FaultCodeReporting";

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FaultCodeReportingService"></param>
		/// <param name="injectConfig"></param>
		/// <param name="loggingService"></param>
		public DeviceConfigFaultCodeReportingController(IDeviceConfigService<DeviceConfigFaultCodeReportingRequest, DeviceConfigFaultCodeReportingDetails> faultCodeReportingService,
			IInjectConfig injectConfig, ILoggingService loggingService) : base(injectConfig.ResolveKeyed<DeviceConfigRequestToAttributeMaps>("DeviceConfigRequestToAttributeMaps"), loggingService)
		{
			base._loggingService.CreateLogger(typeof(DeviceConfigFaultCodeReportingController));
			this._faultCodeReportingService = faultCodeReportingService;
		}

		[HttpPost]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(DeviceConfigFaultCodeReportingResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(DeviceConfigFaultCodeReportingResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(DeviceConfigFaultCodeReportingResponse), (int)HttpStatusCode.InternalServerError)]
		public async Task<ActionResult<DeviceConfigFaultCodeReportingResponse>> Fetch(DeviceConfigRequestBase deviceConfigBaseRequest)
		{
			deviceConfigBaseRequest = await base.ReadRequestContentAsync(deviceConfigBaseRequest);

			DeviceConfigFaultCodeReportingRequest deviceConfigFaultCodeReportingRequest = new DeviceConfigFaultCodeReportingRequest();
			if (deviceConfigBaseRequest == null)
			{
				throw new ArgumentNullException("deviceConfigBaseRequest is null");
			}
			else
			{
				deviceConfigFaultCodeReportingRequest.AssetUIDs = deviceConfigBaseRequest.AssetUIDs;
			}

			deviceConfigFaultCodeReportingRequest.ParameterGroupName = ParameterGroupName;
			deviceConfigFaultCodeReportingRequest.DeviceType = deviceConfigBaseRequest.DeviceType;
			deviceConfigFaultCodeReportingRequest.UserUID = base.GetUserContext(Request);
			deviceConfigFaultCodeReportingRequest.CustomerUID = base.GetCustomerContext(Request);
			deviceConfigFaultCodeReportingRequest.ConfigValues = new Dictionary<string, string>
				{
					{this._attributeMaps.Values["LowSeverityEvents"], string.Empty},
					{this._attributeMaps.Values["MediumSeverityEvents"], string.Empty},
					{this._attributeMaps.Values["HighSeverityEvents"], string.Empty},
					{this._attributeMaps.Values["NextSentEventInHours"], string.Empty},
					{this._attributeMaps.Values["EventDiagnosticFilterInterval"], string.Empty},
					{this._attributeMaps.Values["DiagnosticReportFrequency"], string.Empty}

				};

			this._loggingService.Info("Started invoking FaultCodeReportingService with request : " + JsonConvert.SerializeObject(deviceConfigBaseRequest), "DeviceConfigFaultCodeReportingController.Fetch");

			var response = await this._faultCodeReportingService.Fetch(deviceConfigFaultCodeReportingRequest);

			this._loggingService.Info("Ended invoking FaultCodeReportingService with response : " + JsonConvert.SerializeObject(response), "DeviceConfigFaultCodeReportingController.Fetch");

			return base.SendResponse(HttpStatusCode.OK, new DeviceConfigFaultCodeReportingResponse(response.Lists, response.Errors.OfType<AssetErrorInfo>().ToList()));
		}

		[HttpPut]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(DeviceConfigFaultCodeReportingResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(DeviceConfigFaultCodeReportingResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(DeviceConfigFaultCodeReportingResponse), (int)HttpStatusCode.InternalServerError)]
		public async Task<ActionResult<DeviceConfigFaultCodeReportingResponse>> Save(DeviceConfigFaultCodeReportingRequest deviceConfigFaultCodeReportingRequest)
		{
			deviceConfigFaultCodeReportingRequest = await base.ReadRequestContentAsync(deviceConfigFaultCodeReportingRequest);

			deviceConfigFaultCodeReportingRequest.ParameterGroupName = ParameterGroupName;
			deviceConfigFaultCodeReportingRequest.UserUID = base.GetUserContext(Request);
			deviceConfigFaultCodeReportingRequest.CustomerUID = base.GetCustomerContext(Request);

			deviceConfigFaultCodeReportingRequest.ConfigValues = new Dictionary<string, string>();

			if (deviceConfigFaultCodeReportingRequest.LowSeverityEvents.HasValue)
			{
				deviceConfigFaultCodeReportingRequest.ConfigValues.Add(this._attributeMaps.Values["LowSeverityEvents"], deviceConfigFaultCodeReportingRequest.LowSeverityEvents.ToString());
			}
			if (deviceConfigFaultCodeReportingRequest.MediumSeverityEvents.HasValue)
			{
				deviceConfigFaultCodeReportingRequest.ConfigValues.Add(this._attributeMaps.Values["MediumSeverityEvents"], deviceConfigFaultCodeReportingRequest.MediumSeverityEvents.ToString());
			}
			if (deviceConfigFaultCodeReportingRequest.HighSeverityEvents.HasValue)
			{
				deviceConfigFaultCodeReportingRequest.ConfigValues.Add(this._attributeMaps.Values["HighSeverityEvents"], deviceConfigFaultCodeReportingRequest.HighSeverityEvents.ToString());
			}
			if (deviceConfigFaultCodeReportingRequest.NextSentEventInHours.HasValue && deviceConfigFaultCodeReportingRequest.NextSentEventInHours.Value != -9999999)
			{
				deviceConfigFaultCodeReportingRequest.ConfigValues.Add(this._attributeMaps.Values["NextSentEventInHours"], deviceConfigFaultCodeReportingRequest.NextSentEventInHours.ToString());
			}
			if (deviceConfigFaultCodeReportingRequest.EventDiagnosticFilterInterval.HasValue && deviceConfigFaultCodeReportingRequest.EventDiagnosticFilterInterval.Value != -9999999)
			{
				deviceConfigFaultCodeReportingRequest.ConfigValues.Add(this._attributeMaps.Values["EventDiagnosticFilterInterval"], deviceConfigFaultCodeReportingRequest.EventDiagnosticFilterInterval.ToString());
			}
			if (deviceConfigFaultCodeReportingRequest.DiagnosticReportFrequency.HasValue)
			{
				deviceConfigFaultCodeReportingRequest.ConfigValues.Add(this._attributeMaps.Values["DiagnosticReportFrequency"], deviceConfigFaultCodeReportingRequest.DiagnosticReportFrequency.ToString());
			}

			this._loggingService.Info("Started invoking FaultCodeReportingService with request : " + JsonConvert.SerializeObject(deviceConfigFaultCodeReportingRequest), "DeviceConfigFaultCodeReportingController.Save");

			var response = await this._faultCodeReportingService.Save(deviceConfigFaultCodeReportingRequest);

			this._loggingService.Info("Ended invoking FaultCodeReportingService with response : " + JsonConvert.SerializeObject(response), "DeviceConfigFaultCodeReportingController.Save");

			return base.SendResponse(HttpStatusCode.OK, new DeviceConfigFaultCodeReportingResponse(response.Lists, response.Errors.OfType<AssetErrorInfo>().ToList()));
		}
	}
}
