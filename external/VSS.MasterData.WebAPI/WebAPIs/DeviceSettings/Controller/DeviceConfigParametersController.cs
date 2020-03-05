using ClientModel.DeviceConfig.Request.DeviceConfig.Parameter;
using ClientModel.DeviceConfig.Response.DeviceConfig.Parameter;
using CommonApiLibrary.Filters;
using CommonModel.DeviceSettings.ConfigNameValues;
using CommonModel.Error;
using CommonModel.Exceptions;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using Infrastructure.Service.DeviceConfig.Implementations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Utilities.Logging;

namespace DeviceSettings.Controller
{
	/// <summary>
	/// To Fetch Device Config Parameters
	/// </summary>
	[Route("v1/deviceconfigparameters")]
	public class DeviceConfigParametersController : DeviceConfigApiControllerBase
	{
		private readonly DeviceConfigParmetersServiceBase _deviceConfigParamService;
		private readonly ILoggingService _loggingService;
		/// <summary>
		/// Device Config Parameters initializer
		/// </summary>
		/// <param name="deviceConfigParamService"></param>
		/// <param name="loggingService"></param>
		public DeviceConfigParametersController(DeviceConfigParmetersServiceBase deviceConfigParamService, DeviceConfigAttributeToRequestMaps attributeMaps, ILoggingService loggingService) : base(attributeMaps, loggingService)
		{
			this._deviceConfigParamService = deviceConfigParamService;
			this._loggingService = loggingService;
			this._loggingService.CreateLogger(typeof(DeviceConfigParametersController));
		}

		/// <summary>
		/// To Fetch Device Config Parameters for the given Parameter Group ID and Device Type
		/// </summary>
		/// <param name="deviceType">Device type</param>
		/// <param name="parameterGroupId">Parameter Group ID</param>
		/// <returns></returns>
		[HttpGet]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType(typeof(DeviceConfigParameterResponse), (int)HttpStatusCode.OK)]
		[ProducesResponseType(typeof(DeviceConfigParameterResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(DeviceConfigParameterResponse), (int)HttpStatusCode.InternalServerError)]
		public async Task<ActionResult<DeviceConfigParameterResponse>> Fetch([FromQuery]string deviceType, [FromQuery]ulong parameterGroupId = 0)
		{
			var deviceTypeRequest = parameterGroupId == 0 ? new DeviceConfigParameterRequest
			{
				DeviceType = deviceType,
				IsAssetUIDValidationRequired = false
			} : new DeviceConfigParameterRequest
			{
				DeviceType = deviceType,
				ParameterGroupID = parameterGroupId,
				IsAssetUIDValidationRequired = false
			};

			deviceTypeRequest.CustomerUID = base.GetCustomerContext(this.Request);
			deviceTypeRequest.UserUID = base.GetUserContext(this.Request);
			this._loggingService.Info("Fetching Device Type Parameter Groups for request : " + JsonConvert.SerializeObject(deviceTypeRequest), "DeviceConfigParametersController.Fetch");
			var response = parameterGroupId == 0 ? await _deviceConfigParamService.FetchByDeviceType(deviceTypeRequest) : await this._deviceConfigParamService.Fetch(deviceTypeRequest);

			return base.SendResponse(HttpStatusCode.OK, new DeviceConfigParameterResponse(response.Lists, response.Errors.Cast<ErrorInfo>().ToList()));		
		}

	}
}
