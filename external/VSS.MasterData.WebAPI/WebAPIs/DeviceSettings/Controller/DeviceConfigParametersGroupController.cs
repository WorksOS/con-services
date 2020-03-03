using ClientModel.DeviceConfig.Request.DeviceConfig.ParameterGroup;
using ClientModel.DeviceConfig.Response.DeviceConfig.ParameterGroup;
using CommonApiLibrary.Filters;
using CommonModel.DeviceSettings.ConfigNameValues;
using CommonModel.Error;
using CommonModel.Exceptions;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Utilities.Logging;

namespace DeviceSettings.Controller
{
	[Route("v1/deviceconfigparametergroups")]
	public class DeviceConfigParameterGroupsController : DeviceConfigApiControllerBase
	{
		private readonly IDeviceConfigService<DeviceConfigParameterGroupRequest, ParameterGroupDetails> _deviceConfigParamGroupService;
		private readonly ILoggingService _loggingService;

		public DeviceConfigParameterGroupsController(IDeviceConfigService<DeviceConfigParameterGroupRequest, ParameterGroupDetails> deviceConfigParamGroupService, DeviceConfigAttributeToRequestMaps attributeMap, ILoggingService loggingService) : base(attributeMap, loggingService)
		{
			this._deviceConfigParamGroupService = deviceConfigParamGroupService;
			this._loggingService = loggingService;
			this._loggingService.CreateLogger<DeviceConfigParameterGroupsController>();
		}

		[HttpGet]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(DeviceConfigParameterGroupResponse))]
		[ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(DeviceConfigParameterGroupResponse))]
		[ProducesResponseType((int)HttpStatusCode.InternalServerError, Type = typeof(DeviceConfigParameterGroupResponse))]
		public async Task<ActionResult<DeviceConfigParameterGroupResponse>> Fetch([FromQuery]string deviceType)
		{
			var deviceTypeRequest = new DeviceConfigParameterGroupRequest
			{
				DeviceType = deviceType,
				IsAssetUIDValidationRequired = false
			};

			deviceTypeRequest.CustomerUID = base.GetCustomerContext(this.Request);
			deviceTypeRequest.UserUID = base.GetUserContext(this.Request);
			this._loggingService.Info("Fetching Device Type Parameter Groups for request : " + JsonConvert.SerializeObject(deviceTypeRequest), "DeviceConfigParameterGroupsController.Fetch");
			var response = await this._deviceConfigParamGroupService.Fetch(deviceTypeRequest);

			return base.SendResponse(HttpStatusCode.OK, new DeviceConfigParameterGroupResponse(response.Lists, response.Errors.Cast<ErrorInfo>().ToList()));
		}
	}
}
