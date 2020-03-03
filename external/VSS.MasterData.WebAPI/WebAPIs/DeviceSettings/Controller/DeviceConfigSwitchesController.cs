using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.IOC;
using Utilities.Logging;
using CommonApiLibrary.Filters;
using CommonModel.Error;
using Infrastructure.Common.DeviceSettings.Helpers;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Infrastructure.Common.DeviceSettings.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Infrastructure.Service.DeviceConfig.Implementations;
using ClientModel.DeviceConfig.Request.DeviceConfig.Switches;
using ClientModel.DeviceConfig.Response.DeviceConfig.Switches;
using Infrastructure.Cache.Interfaces;
using Interfaces;
using CommonModel.DeviceSettings.ConfigNameValues;
using System.Reflection;
using CommonModel.Exceptions;
using ClientModel.DeviceConfig.Request.DeviceConfig;

namespace DeviceSettings.Controller
{
	[Route("v1/deviceconfigs/switches")]
	public class DeviceConfigSwitchesController : DeviceConfigApiControllerBase
	{
		private DeviceConfigTemplateBase<DeviceConfigSwitchesRequest, DeviceConfigSwitches> _deviceConfigService;
		private IInjectConfig _iInjectConfig;
		private IParameterAttributeCache _parameterAttributeCache;
		private IDeviceConfigSwitchesService _switchService;
		private IDeviceConfigRepository _deviceConfigRepo;
		public DeviceConfigSwitchesController(
			ILoggingService loggingService,
			DeviceConfigTemplateBase<DeviceConfigSwitchesRequest, DeviceConfigSwitches> deviceConfigService,
			IInjectConfig injectConfig, IParameterAttributeCache parameterAttributeCache,
			IDeviceConfigSwitchesService switchService, IDeviceConfigRepository deviceRepo
		) : base(injectConfig.ResolveKeyed<DeviceConfigRequestToAttributeMaps>("DeviceConfigRequestToAttributeMaps"),
			loggingService)
		{
			_loggingService.CreateLogger(typeof(DeviceConfigSwitchesController));
			_deviceConfigService = deviceConfigService;
			_iInjectConfig = injectConfig;
			_parameterAttributeCache = parameterAttributeCache;
			_switchService = switchService;
			_deviceConfigRepo = deviceRepo;
		}

		[HttpPost]
		[Route("DualStateSwitches")]
		[UserUidParser]
		[ProducesResponseType(typeof(DeviceConfigConfiguredDualStateSwitchesResponse), (int) HttpStatusCode.OK)]
		[ProducesResponseType(typeof(DeviceConfigConfiguredDualStateSwitchesResponse), (int)HttpStatusCode.BadRequest)]
		[ProducesResponseType(typeof(DeviceConfigConfiguredDualStateSwitchesResponse), (int)HttpStatusCode.InternalServerError)]
		public async Task<ActionResult<DeviceConfigConfiguredDualStateSwitchesResponse>> GetDualStateSwitches(DeviceConfigRequestBase request)
		{
			request = await base.ReadRequestContentAsync<DeviceConfigRequestBase>(request);

			if (request == null)
				throw new DomainException { Error = new ErrorInfo { ErrorCode = (int)ErrorCodes.RequestInvalid, Message = Utils.GetEnumDescription(ErrorCodes.RequestInvalid), IsInvalid = true } };

			var switchesRequest = new DeviceConfigSwitchesRequest();
			_loggingService.Info("Request Processing Started", MethodBase.GetCurrentMethod().Name);
			var parameterNames = _iInjectConfig.ResolveKeyed<DeviceConfigParameterNames>("DeviceConfigParameterNames");
			switchesRequest.ParameterGroupName = Convert.ToString(parameterNames.Values["SwitchesParameterGroupName"]);
			switchesRequest.AssetUIDs = request.AssetUIDs;
			switchesRequest.DeviceType = request.DeviceType;
			switchesRequest.UserUID = base.GetUserContext(Request);
			switchesRequest.CustomerUID = base.GetCustomerContext(Request);
			switchesRequest.ConfigValues = new Dictionary<string, string>();
			_loggingService.Info("About to Fetch From Database", MethodBase.GetCurrentMethod().Name);
			var response = await _switchService.GetConfiguredDualStateSwitches(switchesRequest);
			return base.SendResponse(HttpStatusCode.OK,
				new DeviceConfigConfiguredDualStateSwitchesResponse(response.Lists,
					response.Errors.Cast<AssetErrorInfo>().ToList()));

		}

		[HttpPost]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType( (int) HttpStatusCode.OK, Type = typeof(DeviceConfigSwitchesResponse))]
		[ProducesResponseType( (int) HttpStatusCode.BadRequest, Type = typeof(DeviceConfigSwitchesResponse))]
		[ProducesResponseType( (int) HttpStatusCode.InternalServerError, Type = typeof(DeviceConfigSwitchesResponse))]
		public async Task<ActionResult<DeviceConfigSwitchesResponse>> Fetch(DeviceConfigRequestBase request, [FromQuery] int switchNumber = 0)
		{
			request = await base.ReadRequestContentAsync<DeviceConfigRequestBase>(request);

			if (request == null)
				throw new DomainException { Error = new ErrorInfo { ErrorCode = (int)ErrorCodes.RequestInvalid, Message = Utils.GetEnumDescription(ErrorCodes.RequestInvalid), IsInvalid = true } };

			_loggingService.Info("Request Processing Started", MethodBase.GetCurrentMethod().Name);
			var switchesRequest = new DeviceConfigSwitchesRequest();
			var parameterNames = _iInjectConfig.ResolveKeyed<DeviceConfigParameterNames>("DeviceConfigParameterNames");
			switchesRequest.ParameterGroupName =
				Convert.ToString(parameterNames.Values["SwitchesParameterGroupName"]);
			switchesRequest.AssetUIDs = request.AssetUIDs;
			switchesRequest.DeviceType = request.DeviceType;
			switchesRequest.UserUID = base.GetUserContext(Request);
			switchesRequest.CustomerUID = base.GetCustomerContext(Request);
			switchesRequest.ConfigValues = new Dictionary<string, string>();
			switchesRequest.SwitchNumber = switchNumber;
			_loggingService.Info("About to Fetch From Database", MethodBase.GetCurrentMethod().Name);
			var response = await _deviceConfigService.Fetch(switchesRequest);
			return base.SendResponse(HttpStatusCode.OK, new DeviceConfigSwitchesResponse(response.Lists.Select(list => list as DeviceConfigSwitches),
					response.Errors.Select(error => error as AssetErrorInfo).ToList()));
		}

		[HttpPut]
		[Route("")]
		[UserUidParser]
		[ProducesResponseType( (int) HttpStatusCode.OK, Type = typeof(DeviceConfigSwitchesResponse))]
		[ProducesResponseType( (int) HttpStatusCode.BadRequest, Type = typeof(DeviceConfigSwitchesResponse))]
		[ProducesResponseType( (int) HttpStatusCode.InternalServerError, Type = typeof(DeviceConfigSwitchesResponse))]
		public async Task<ActionResult<DeviceConfigSwitchesResponse>> Save(DeviceConfigSwitchesRequest request)
		{
			request = await base.ReadRequestContentAsync<DeviceConfigSwitchesRequest>(request);

			if (request == null)
				throw new DomainException { Error = new ErrorInfo { ErrorCode = (int)ErrorCodes.RequestInvalid, Message = Utils.GetEnumDescription(ErrorCodes.RequestInvalid), IsInvalid = true } };

			var switchesRequest = new DeviceConfigSwitchesRequest();
			//GetDeviceParameterName from ParameterName Collection and Populate the values for it as it will be used for key for attribute value mapping
			var parameterNames = _iInjectConfig.ResolveKeyed<DeviceConfigParameterNames>("DeviceConfigParameterNames");
			//This is must because we are using it as key for getting data from parameter cache.
			switchesRequest.ParameterGroupName =
				Convert.ToString(parameterNames.Values["SwitchesParameterGroupName"]);
			switchesRequest.AssetUIDs = request.AssetUIDs;
			switchesRequest.DeviceType = request.DeviceType;
			switchesRequest.SingleStateSwitches = (request as DeviceConfigSwitchesRequest).SingleStateSwitches ?? new List<DeviceConfigSingleStateSwitchRequest>();
			switchesRequest.DualStateSwitches = (request as DeviceConfigSwitchesRequest).DualStateSwitches ?? new List<DeviceConfigDualStateSwitchRequest>();
			switchesRequest.UserUID = base.GetUserContext(Request);
			switchesRequest.CustomerUID = base.GetCustomerContext(Request);
			switchesRequest.ConfigValues = new Dictionary<string, string>();
			IList<ErrorInfo> errors = new List<ErrorInfo>();
			_loggingService.Info("About To Start Business layer for save", MethodBase.GetCurrentMethod().Name);
			var response = await _deviceConfigService.Save(switchesRequest);
			return base.SendResponse(HttpStatusCode.OK, new DeviceConfigSwitchesResponse(response.Lists.Select(list => list as DeviceConfigSwitches),
				response.Errors.Select(error => error as AssetErrorInfo).ToList()));
		}
	}
}
