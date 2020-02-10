using AutoMapper;
using ClientModel.DeviceConfig.Request;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.Switches;
using ClientModel.DeviceConfig.Response.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig.DeviceTypeGroupParameterAttribute;
using ClientModel.DeviceConfig.Response.DeviceConfig.Switches;
using CommonModel.DeviceSettings.ConfigNameValues;
using CommonModel.Error;
using CommonModel.Exceptions;
using DbModel.DeviceConfig;
using Infrastructure.Cache.Interfaces;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Service.DeviceAcknowledgementByPasser.Interfaces;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.IOC;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;
using Configurations = CommonModel.DeviceSettings.Configurations;
using DeviceConfigurationDefaultsAndConfigurations = CommonModel.DeviceSettings.DeviceConfigurationDefaultsAndConfigurations;

namespace Infrastructure.Service.DeviceConfig.Implementations
{
	public class DeviceConfigSwitchesService : DeviceConfigTemplateBase<DeviceConfigSwitchesRequest, DeviceConfigSwitches>, IDeviceConfigSwitchesService
	{
		private readonly ISwitchesValidator _switchesValidtor;
		private readonly ConfigNameValueCollection _requestToAttributeMaps;
		private readonly ConfigNameValueCollection _attributeToRequestMaps;
        private readonly List<string> _sendAllSwitchesDeviceFamilyLists;

        public DeviceConfigSwitchesService(
            IEnumerable<IRequestValidator<IServiceRequest>> requestInvalidateValidators,
            IEnumerable<IRequestValidator<DeviceConfigRequestBase>> commonDeviceLevelValidators,
            ISwitchesValidator switchesValidator,
            IInjectConfig injectConfig,
            IDeviceConfigRepository deviceConfigRepository,
            IParameterAttributeCache parameterAttributeCache,
            IMessageConstructor messageConstructor,
            IAckBypasser ackBypasser, IDeviceConfigSettingConfig settingsConfig,
            IAssetDeviceRepository assetDeviceRepository,
			IOptions<Configurations> configurations,
			IMapper mapper, ILoggingService loggingService, ITransactions transactions) :
            base(injectConfig, deviceConfigRepository, parameterAttributeCache, mapper, loggingService, requestInvalidateValidators, commonDeviceLevelValidators, 
				assetDeviceRepository, messageConstructor, ackBypasser, configurations, settingsConfig, transactions)
        {
            _attributeToRequestMaps = injectConfig.ResolveKeyed<DeviceConfigAttributeToRequestMaps>("DeviceConfigAttributeToRequestMaps");
            _requestToAttributeMaps = injectConfig.ResolveKeyed<DeviceConfigRequestToAttributeMaps>("DeviceConfigRequestToAttributeMaps");
            _switchesValidtor = switchesValidator;
            _sendAllSwitchesDeviceFamilyLists = configurations.Value.AppSettings.DeviceConfigSendAllSwitchesDeviceFamilyLists.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        protected async override Task initRequest(DeviceConfigSwitchesRequest request)
        {
            var parameters = await _parameterAttributeCache.Get(request.DeviceType, request.ParameterGroupName);

            var deviceTypeParametersToBeWorkedOn = parameters.Select(param => new
            {
                param.DeviceTypeParameterID,
                param.AttributeID,
                param.ParameterName,
                param.AttributeName,
                param.DeviceParamAttrID,
                param.DefaultValueJSON
            }).ToList().Distinct();


            var paramNames = request.SingleStateSwitches.Any() && request.DualStateSwitches.Any() ?
                                request.SingleStateSwitches.Select(sSwitch => sSwitch.SwitchParameterName).Union(request.DualStateSwitches.Select(sSwitch => sSwitch.SwitchParameterName)) :
                                request.SingleStateSwitches.Any() && !request.DualStateSwitches.Any() ? request.SingleStateSwitches.Select(sSwitch => sSwitch.SwitchParameterName) :
                                !request.SingleStateSwitches.Any() && request.DualStateSwitches.Any() ? request.DualStateSwitches.Select(sSwitch => sSwitch.SwitchParameterName) :
                                null;

            if (paramNames.Any())
            {
                var distinctParamNames = (deviceTypeParametersToBeWorkedOn.ToList())
                    .Select(param => param.ParameterName).Distinct().ToList();

                //Check for Invalid Parameters
                var invalidParams = paramNames.Any(param => !distinctParamNames.Contains(param)) ?
                                    paramNames.Where(param => !distinctParamNames.Contains(param)).Select(param => param) :
                                    new List<string>();



                foreach (var parameter in paramNames)
                {
                    var singleStateSwitch =
                        request.SingleStateSwitches.Where(
                            switches => string.Compare(switches.SwitchParameterName, parameter) == 0);
                    if (singleStateSwitch.Any())
                    {

                        foreach (var attribute in deviceTypeParametersToBeWorkedOn.ToList()
                            .Where(param => String.Compare(parameter, param.ParameterName,
                                                StringComparison.OrdinalIgnoreCase) == 0)
                            .Select(param => new
                            {
                                param.AttributeName,
                                param.DeviceTypeParameterID,
                                param.AttributeID,
                                param.DeviceParamAttrID,
                                param.DefaultValueJSON
                            }))
                        {
                            var switchInfo = singleStateSwitch.First();
                            var propertyInfo = switchInfo.GetType().GetProperty(attribute.AttributeName);
                            if (propertyInfo != null)
                            {
                                var value = propertyInfo.GetValue(switchInfo) ?? string.Empty;
                                request.ConfigValues.Add(
                                    string.Format("{0}.{1}", parameter,
                                        attribute.AttributeName), Convert.ToString(value));
                            }
                            else //TODO: Issue for MonitoredWhen, Remove this condition and make it to use ConfigNameValueCollection(RequestToAttributeMaps)
                            {
                                if (_requestToAttributeMaps.Values.ContainsKey(attribute.AttributeName))
                                {
                                    var missedPropertyInfo = switchInfo.GetType().GetProperty(_requestToAttributeMaps.Values[attribute.AttributeName]);
                                    var value = missedPropertyInfo.GetValue(switchInfo) ?? string.Empty;
                                    request.ConfigValues.Add(
                                        string.Format("{0}.{1}", parameter,
                                            attribute.AttributeName), Convert.ToString(value));
                                }
                            }
                        }
                    }

                    var dualStateSwitch =
                        request.DualStateSwitches.Where(
                            switches => String.CompareOrdinal(switches.SwitchParameterName, parameter) == 0);
                    var deviceConfigDualStateSwitchRequests = dualStateSwitch as IList<DeviceConfigDualStateSwitchRequest> ?? dualStateSwitch.ToList();
                    if (deviceConfigDualStateSwitchRequests.Any())
                    {
                        foreach (var attribute in deviceTypeParametersToBeWorkedOn.ToList()
                            .Where(param => String.Compare(parameter, param.ParameterName, StringComparison.OrdinalIgnoreCase) == 0)
                            .Select(param => new
                            {
                                param.AttributeName,
                                param.DeviceTypeParameterID,
                                param.AttributeID,
                                param.DeviceParamAttrID,
                                param.DefaultValueJSON
                            }))
                        {
                            var switchesConfig = JsonConvert.DeserializeObject<DeviceConfigurationDefaultsAndConfigurations>(attribute.DefaultValueJSON);
                            if (!switchesConfig.Configurations.SwitchesConfig.isTampered)
                            {
                                var switchInfo = deviceConfigDualStateSwitchRequests.First();
                                var propertyInfo = switchInfo.GetType().GetProperty(attribute.AttributeName);
                                if (propertyInfo != null)
                                {
                                    var value = switchInfo.GetType().GetProperty(attribute.AttributeName)
                                                    .GetValue(switchInfo) ?? string.Empty;
                                    request.ConfigValues.Add(
                                        string.Format("{0}.{1}", parameter,
                                            attribute.AttributeName), Convert.ToString(value));
                                }
                                else //TODO: Issue for MonitoredWhen, Remove this condition and make it to use ConfigNameValueCollection(RequestToAttributeMaps)
                                {
                                    if (_requestToAttributeMaps.Values.ContainsKey(attribute.AttributeName))
                                    {
                                        var missedPropertyInfo = switchInfo.GetType().GetProperty(_requestToAttributeMaps.Values[attribute.AttributeName]);
                                        var value = missedPropertyInfo.GetValue(switchInfo) ?? string.Empty;
                                        request.ConfigValues.Add(
                                            string.Format("{0}.{1}", parameter,
                                                attribute.AttributeName), Convert.ToString(value));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected async override Task<IList<IErrorInfo>> DoParameterSpecificValidation(DeviceConfigSwitchesRequest request)
        {
            return await _switchesValidtor.Validate(request);
        }

		protected async override Task<IList<DeviceConfigSwitches>> GetResponse(DeviceConfigSwitchesRequest request, IList<DeviceConfigDto> deviceConfigDto)
		{
			_loggingService.Info("Inside GetSwitchesResponse to Cast the response to particular object", "DeviceConfigTemplateBase.Fetch");
			var parameters = await _parameterAttributeCache.Get(request.DeviceType);
			IList<DeviceConfigSwitches> switchesResponse = new List<DeviceConfigSwitches>();
			DeviceConfigSwitches switchResponse = null;

			foreach (var assetUId in request.AssetUIDs)
			{
				var deviceTypeParametersToBeWorkedOn = parameters.Where(param => String.Compare(param.GroupName, "Switches", StringComparison.OrdinalIgnoreCase) == 0
						&& String.Compare(param.TypeName, request.DeviceType, StringComparison.OrdinalIgnoreCase) == 0)
						.Select(param => new { param.DeviceTypeParameterID, param.DefaultValueJSON, param.ParameterName }).ToList().Distinct();

				switchResponse = new DeviceConfigSwitches();

				switchResponse.DualStateSwitch = new List<DeviceConfigDualStateSwitch>();
				switchResponse.SingleStateSwitch = new List<DeviceConfigSingleStateSwitch>();

				foreach (var parameter in deviceTypeParametersToBeWorkedOn)
				{
					var deviceInfo = deviceConfigDto
									.Where(dI => dI.DeviceTypeParameterID == parameter.DeviceTypeParameterID && string.Compare(dI.AssetUIDString, assetUId.Replace("-",string.Empty), StringComparison.OrdinalIgnoreCase) == 0)
									.Select(dI => new { dI.DeviceParameterAttributeId, dI.AttributeName, dI.AttributeValue, UpdateUTC = dI.UpdateUTC, dI.AssetUIDString });

					var switchParams = JsonConvert.DeserializeObject<DeviceConfigurationDefaultsAndConfigurations>(parameter.DefaultValueJSON);

					if (switchParams.Configurations.SwitchesConfig.isSingleState)
					{
						var singleStateSwitch = (DeviceConfigSingleStateSwitch)Activator.CreateInstance(typeof(DeviceConfigSingleStateSwitch));
						if (deviceInfo.Any())
						{
							deviceInfo.ToList().ForEach(info =>
										{
											try
											{

												var property = singleStateSwitch.GetType().GetProperty(info.AttributeName);
												//TODO: Issue for MonitoredWhen, Remove this condition and make it to use ConfigNameValueCollection(RequestToAttributeMaps)
												if (property == null)
												{
													if (_attributeToRequestMaps.Values.ContainsKey(info.AttributeName))
													{
														property = singleStateSwitch.GetType().GetProperty(_attributeToRequestMaps.Values[info.AttributeName]);
													}
												}
												if (!string.IsNullOrEmpty(info.AttributeValue))
												{
													property.SetValue(singleStateSwitch, Convert.ChangeType(info.AttributeValue, property.PropertyType));
												}
											}
											catch (NullReferenceException e)
											{
											}
										});
							singleStateSwitch.SwitchNumber = switchParams.Configurations.SwitchesConfig.SwitchNumber;
							singleStateSwitch.SwitchParameterName = parameter.ParameterName;
							switchResponse.SingleStateSwitch.Add(singleStateSwitch);
						}
					}
					else
					{
						var dualStateSwitch = (DeviceConfigDualStateSwitch)Activator.CreateInstance(typeof(DeviceConfigDualStateSwitch));
						if (deviceInfo.Any())
						{
							deviceInfo.ToList().ForEach(info =>
							{
								try
								{
									PropertyInfo property = dualStateSwitch.GetType().GetProperty(info.AttributeName);
									//TODO: Issue for MonitoredWhen, Remove this condition and make it to use ConfigNameValueCollection(RequestToAttributeMaps)
									if (property == null)
									{
										if (_attributeToRequestMaps.Values.ContainsKey(info.AttributeName))
										{
											property = dualStateSwitch.GetType().GetProperty(_attributeToRequestMaps.Values[info.AttributeName]);
										}
									}
									if (!string.IsNullOrEmpty(info.AttributeValue))
									{
										property.SetValue(dualStateSwitch, Convert.ChangeType(info.AttributeValue, property.PropertyType));
									}
								}
								catch (NullReferenceException e)
								{
								}
							});
							dualStateSwitch.SwitchNumber = switchParams.Configurations.SwitchesConfig.SwitchNumber;
							dualStateSwitch.SwitchParameterName = parameter.ParameterName;
							switchResponse.DualStateSwitch.Add(dualStateSwitch);
						}
					}

					if (deviceInfo.Any())
					{
						switchResponse.LastUpdatedOn = Convert.ToDateTime(deviceInfo.First().UpdateUTC);
						switchResponse.AssetUID = Guid.Parse(deviceInfo.First().AssetUIDString);
					}
					else
					{
						switchResponse.AssetUID = Guid.Parse(assetUId);
					}
				}
				switchesResponse.Add(switchResponse);
			}

            //For filtering by SwitchNumber
            if (request.SwitchNumber > 0)
            {
                var singleStateWithMatchingSwitchNumbers = switchesResponse.First().SingleStateSwitch.Where(ssSwitch => ssSwitch.SwitchNumber == request.SwitchNumber).ToList();
                var dualStateWithMatchingSwitchNumbers = switchesResponse.First().DualStateSwitch.Where(ssSwitch => ssSwitch.SwitchNumber == request.SwitchNumber).ToList();

                switchesResponse.First().SingleStateSwitch = singleStateWithMatchingSwitchNumbers;
                switchesResponse.First().DualStateSwitch = dualStateWithMatchingSwitchNumbers;
            }

            return switchesResponse;
        }

        private int GetCaptionStateForDualStateSwitches(DeviceConfigDualStateSwitch dualStateSwitch)
        {
            if (!string.IsNullOrEmpty(dualStateSwitch.SwitchOpen) || !string.IsNullOrEmpty(dualStateSwitch.SwitchClosed))
                return (int)SwitchCaption.Configured;
            return (int)SwitchCaption.Unconfigured;
        }

        private int GetCaptionStateForSingleStateSwitches(SwitchActiveState activeState)
        {
            switch (activeState)
            {
                case SwitchActiveState.NormallyOpen:
                    return (int)SwitchCaption.Open;
                case SwitchActiveState.NormallyClosed:
                    return (int)SwitchCaption.Close;
                case SwitchActiveState.NotConfigured:
                    return (int)SwitchCaption.Configured;
                case SwitchActiveState.NotInstalled:
                    return (int)SwitchCaption.Configured;
                default:
                    return (int)SwitchCaption.Unconfigured;
            }
        }

        public async Task<DeviceConfigServiceResponse<DeviceConfigConfiguredDualStateSwitchInfo>> GetConfiguredDualStateSwitches(DeviceConfigSwitchesRequest request)
        {
            _loggingService.Info("GetDConfigured Dual Switches Called", MethodBase.GetCurrentMethod().Name);
            var errors = new List<IErrorInfo>();
            List<DeviceConfigConfiguredDualStateSwitchInfo> response = new List<DeviceConfigConfiguredDualStateSwitchInfo>();

            errors.AddRange(await base.DoValidation(request));

            //var propNames = singleStateSwitch.GetType().GetProperties().Select(props => props.Name);
            var validationResult = await DoValidationSpecificToDualStateSwitches(request);

            if (validationResult.Count > 0)
            {
                //Process the errors if it is there
                errors.AddRange(validationResult);
                _loggingService.Info(JsonConvert.SerializeObject(errors.Select(error => error as AssetErrorInfo)), "DeviceConfigTemplateBase.Fetch");
                throw new DomainException { Errors = errors };
            }

            var deviceConfigDto = await base.Fetch(request as DeviceConfigRequestBase);

            response.Add(await GetConfiguredDualStateSwitchesResponse(request, deviceConfigDto));

            return new DeviceConfigServiceResponse<DeviceConfigConfiguredDualStateSwitchInfo>(response, errors);
        }

        public async Task<IList<IErrorInfo>> DoValidationSpecificToDualStateSwitches(DeviceConfigSwitchesRequest request)
        {
            return await _switchesValidtor.ValidateDualStateSwitches(request);
        }

        private async Task<DeviceConfigConfiguredDualStateSwitchInfo> GetConfiguredDualStateSwitchesResponse(DeviceConfigSwitchesRequest request, IList<DeviceConfigDto> deviceConfigDto)
        {
            _loggingService.Info("Inside GetSwitchesResponse to Cast the response to particular object", "DeviceConfigTemplateBase.Fetch");
            DeviceConfigConfiguredDualStateSwitchInfo switchResponse = new DeviceConfigConfiguredDualStateSwitchInfo();

            var parameters = await _parameterAttributeCache.Get(request.DeviceType, request.ParameterGroupName);

            var deviceTypeParametersToBeWorkedOn = parameters.Where(param => string.Compare(param.GroupName, "Switches", true) == 0
                        && string.Compare(param.TypeName, request.DeviceType, true) == 0)
                        .Select(param => new { param.DeviceTypeParameterID, param.DefaultValueJSON, param.ParameterName }).ToList().Distinct();

            List<DeviceConfigConfiguredDualStateSwitches> dualStateSwitches = new List<DeviceConfigConfiguredDualStateSwitches>();

            foreach (var parameter in deviceTypeParametersToBeWorkedOn)
            {
                var deviceInfo = deviceConfigDto
                                .Where(dI => dI.DeviceTypeParameterID == parameter.DeviceTypeParameterID)
                                .Select(dI => new { dI.DeviceParameterAttributeId, dI.AttributeName, dI.AttributeValue, dI.AssetUID });

                if (JsonConvert.DeserializeObject<DeviceConfigurationDefaultsAndConfigurations>(parameter.DefaultValueJSON).Configurations.SwitchesConfig.isSingleState)
                    continue;
                else
                {
                    var dualStateSwitch = (DeviceConfigConfiguredDualStateSwitches)Activator.CreateInstance(typeof(DeviceConfigConfiguredDualStateSwitches));
                    if (deviceInfo.Any())
                    {
                        deviceInfo.ToList().ForEach(info =>
                        {
                            try
                            {
                                if (dualStateSwitch.GetType().GetProperties().Where(propName => propName.Name == info.AttributeName).Any())
                                {
                                    PropertyInfo property = dualStateSwitch.GetType().GetProperty(info.AttributeName);
									if (!string.IsNullOrEmpty(info.AttributeValue))
									{
										property.SetValue(dualStateSwitch, Convert.ChangeType(info.AttributeValue, property.PropertyType));
									}
                                }
                            }
                            catch (NullReferenceException e)
                            {
                            }
                        });
                        dualStateSwitch.SwitchNumber = JsonConvert.DeserializeObject<DeviceConfigurationDefaultsAndConfigurations>(parameter.DefaultValueJSON).Configurations.SwitchesConfig.SwitchNumber;
                    }
                    //switchInfo.state = dualStateSwitch;
                    if (dualStateSwitch.SwitchEnabled)
                    {
                        dualStateSwitches.Add(dualStateSwitch);
                    }
                }
            }
            return new DeviceConfigConfiguredDualStateSwitchInfo { Switches = dualStateSwitches, AssetUID = Guid.Parse(request.AssetUIDs.First()) };
        }

        protected async override Task<DeviceConfigRequestBase> GetRequestAndHandleForNullCases(DeviceConfigRequestBase request)
        {
            if (_deviceTypeFamilyContainer.ContainsKey(request.DeviceType) && !_sendAllSwitchesDeviceFamilyLists.Contains(_deviceTypeFamilyContainer[request.DeviceType].FamilyName))
                return request;

            var switchRequest = request as DeviceConfigSwitchesRequest;
            var parameters = await _parameterAttributeCache.Get(request.DeviceType);

            var deviceTypeParametersToBeWorkedOn = parameters.Where(param => String.Compare(param.GroupName, "Switches", StringComparison.OrdinalIgnoreCase) == 0
                        && String.Compare(param.TypeName, request.DeviceType, StringComparison.OrdinalIgnoreCase) == 0)
                        .Select(param => new { param.DeviceTypeParameterID, param.DefaultValueJSON, param.ParameterName }).ToList().Distinct();

            foreach (var parameter in deviceTypeParametersToBeWorkedOn)
            {
                var switchParams = JsonConvert.DeserializeObject<DeviceConfigurationDefaultsAndConfigurations>(parameter.DefaultValueJSON);
                if (!switchParams.Configurations.SwitchesConfig.isSingleState && !switchRequest.DualStateSwitches.Any(dSS => string.Compare(dSS.SwitchParameterName, parameter.ParameterName, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    var dualStateSwitch = new DeviceConfigDualStateSwitchRequest();
                    dualStateSwitch.SwitchNumber = switchParams.Configurations.SwitchesConfig.SwitchNumber;
                    dualStateSwitch.SwitchMonitoringStatus = "";
                    dualStateSwitch.SwitchClosed = "";
                    dualStateSwitch.SwitchEnabled = false;
                    dualStateSwitch.SwitchName = "";
                    dualStateSwitch.SwitchOpen = "";
                    dualStateSwitch.SwitchParameterName = parameter.ParameterName;
                    dualStateSwitch.SwitchSensitivity = 0;
                    switchRequest.DualStateSwitches.Add(dualStateSwitch);
                }
            }

            return switchRequest;
        }

        protected override bool isAttributeNameExists(IList<DeviceConfigDto> deviceConfigResponseDtos, KeyValuePair<string, DeviceTypeGroupParameterAttributeDetails> attributeId, string deviceUID)
        {
            return deviceConfigResponseDtos.Where(x => string.Compare(x.DeviceUIDString, deviceUID) == 0).Any(x => x.AttributeName == attributeId.Value.AttributeName && x.ParameterName == attributeId.Value.ParameterName);
        }
    }
}
