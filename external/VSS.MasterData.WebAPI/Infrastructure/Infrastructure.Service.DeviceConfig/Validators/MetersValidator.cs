using CommonModel.AssetSettings;
using ClientModel.DeviceConfig.Request.DeviceConfig.Meters;
using CommonModel.Error;
using DbModel.DeviceConfig;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Logging;
using Infrastructure.Cache.Interfaces;

namespace Infrastructure.Service.DeviceConfig.Validators
{
	public class MetersValidator : RequestValidatorBase, IRequestValidator<DeviceConfigMetersRequest>
    {
        private readonly IParameterAttributeCache _parameterAttributeCache;
        private readonly IAssetDeviceRepository _assetDeviceRepository;
        private readonly IDeviceConfigRepository _deviceConfigRepository;

        public MetersValidator(IParameterAttributeCache parameterAttributeCache, IDeviceConfigRepository deviceConfigRepository, IAssetDeviceRepository assetDeviceRepository, ILoggingService loggingService) : base(loggingService)
        {
            this._parameterAttributeCache = parameterAttributeCache;
            this._assetDeviceRepository = assetDeviceRepository;
            this._deviceConfigRepository = deviceConfigRepository;
        }

        public async Task<IList<IErrorInfo>> Validate(DeviceConfigMetersRequest request)
        {
            List<IErrorInfo> result = new List<IErrorInfo>();

            if (request.SmhOdometerConfig.HasValue)
            {
                if (!Enum.IsDefined(typeof(MetersSmhOdometerConfig), request.SmhOdometerConfig.Value))
                {
                    result.Add(base.GetValidationResult(ErrorCodes.MetersInvalidSmhOdometerConfig, string.Format(Utils.GetEnumDescription(ErrorCodes.MetersInvalidSmhOdometerConfig), Utils.GetEnumValuesAsKeyValueString(typeof(MetersSmhOdometerConfig))), true, "MetersValidator.Validate"));
                }
            }

            if (request.HoursMeter != null)
            {
                if (request.HoursMeter.ProposedValue.HasValue && request.HoursMeter.ProposedValue < 0 || request.HoursMeter.ProposedValue > 210554060)
                {
                    result.Add(base.GetValidationResult(ErrorCodes.MetersInvalidProposedValueForHourMeter, string.Format(Utils.GetEnumDescription(ErrorCodes.MetersInvalidProposedValueForHourMeter), 0, 210554060), true, "MetersValidator.Validate"));
                }

                if (request.HoursMeter.CurrentValue.HasValue && (request.HoursMeter.CurrentValue < 0 || request.HoursMeter.CurrentValue.Value > 210554060))
                {
                    result.Add(base.GetValidationResult(ErrorCodes.MetersInvalidCurrentValueForHoursMeter, string.Format(Utils.GetEnumDescription(ErrorCodes.MetersInvalidCurrentValueForHoursMeter), 0, 210554060), true, "MetersValidator.Validate"));
                }

                if (request.HoursMeter.ProposedValue.HasValue && Utils.GetPrecisionCount(Convert.ToDecimal(request.HoursMeter.ProposedValue.Value)) > 2)
                {
                    result.Add(base.GetValidationResult(ErrorCodes.MetersInvalidPrecisionValueForHourMeter, string.Format(Utils.GetEnumDescription(ErrorCodes.MetersInvalidPrecisionValueForHourMeter), 2), true, "MetersValidator.Validate"));
                }
            }

            if (request.OdoMeter != null)
            {
                if (request.OdoMeter.ProposedValue.HasValue && request.OdoMeter.ProposedValue < 0 || request.OdoMeter.ProposedValue.Value > 1677721.53)
                {
                    result.Add(base.GetValidationResult(ErrorCodes.MetersInvalidProposedValueForOdoMeter, string.Format(Utils.GetEnumDescription(ErrorCodes.MetersInvalidProposedValueForOdoMeter), 0, 1677721.53), true, "MetersValidator.Validate"));
                }

                if (request.OdoMeter.CurrentValue.HasValue && (request.OdoMeter.CurrentValue < 0 || request.OdoMeter.CurrentValue.Value > 1677721.53))
                {
                    result.Add(base.GetValidationResult(ErrorCodes.MetersInvalidCurrentValueForOdoMeter, string.Format(Utils.GetEnumDescription(ErrorCodes.MetersInvalidCurrentValueForOdoMeter), 0, 1677721.53), true, "MetersValidator.Validate"));
                }
            }

            var cachedParameterAttributes = await this._parameterAttributeCache.Get(request.DeviceType, request.ParameterGroupName);

            if (cachedParameterAttributes != null && cachedParameterAttributes.Any())
            {
                var hoursMeter = cachedParameterAttributes.FirstOrDefault(x => x.AttributeName == "HoursMeter");
                var smhOdometerConfig = cachedParameterAttributes.FirstOrDefault(x => x.AttributeName == "SMHOdometerConfig");
                var hourMeterKeys = request.ConfigValues.Keys.Where(x => x.Contains("HoursMeter")).ToList();
                bool hourMeterRemoval = false;

                if (hoursMeter != null && request.HoursMeter != null && request.HoursMeter.ProposedValue != null)
                {
                    var deviceConfigurationValues = JsonConvert.DeserializeObject<DeviceConfigurationDefaultsAndConfigurations>(hoursMeter.DefaultValueJSON);

                    if (deviceConfigurationValues != null && deviceConfigurationValues.Configurations.MetersConfig != null)
                    {
                        //Hours meter backvalue updation
                        if (request.HoursMeter != null && request.HoursMeter.CurrentValue.HasValue && request.HoursMeter.ProposedValue.HasValue && (request.HoursMeter.ProposedValue < request.HoursMeter.CurrentValue && !deviceConfigurationValues.Configurations.MetersConfig.AllowBackward))
                        {
                            hourMeterRemoval = true;
                            result.Add(base.GetValidationResult(ErrorCodes.MetersBackwardUpdationNotAllowedForHoursMeter, Utils.GetEnumDescription(ErrorCodes.MetersBackwardUpdationNotAllowedForHoursMeter), false, "MetersValidator.Validate"));
                        }

                        if (cachedParameterAttributes.Any(x => x.ParameterName == "SMHOdometerConfig"))
                        {
                            if (request.SmhOdometerConfig.Value == MetersSmhOdometerConfig.J1939)
                            {
                                hourMeterRemoval = true;
                                result.Add(base.GetValidationResult(ErrorCodes.MetersUpdationNotAllowedForHoursMeter, Utils.GetEnumDescription(ErrorCodes.MetersUpdationNotAllowedForHoursMeter), false, "MetersValidator.Validate"));
                            }
                            else
                            {
                                var smhOdometerValues = await this._deviceConfigRepository.Fetch(request.AssetUIDs, 
									new List<DeviceConfigDto>
									{
										new DeviceConfigDto
										{
											DeviceParameterAttributeId = smhOdometerConfig.DeviceParamAttrID,
											DeviceTypeParameterID = smhOdometerConfig.DeviceTypeParameterID
										}
									}
								);

                                //Dont update if Service meter is J1939
                                var smhOdometerValue = smhOdometerValues.FirstOrDefault();
                                if (smhOdometerValue != null)
                                {
                                    if (string.Compare(smhOdometerValue.AttributeValue, MetersSmhOdometerConfig.J1939.ToString(), true) == 0)
                                    {
                                        hourMeterRemoval = true;
                                        result.Add(base.GetValidationResult(ErrorCodes.MetersUpdationNotAllowedForHoursMeter, Utils.GetEnumDescription(ErrorCodes.MetersUpdationNotAllowedForHoursMeter), false, "MetersValidator.Validate"));
                                    }
                                }
                            }
                        }
                        if (request.AssetUIDs != null && request.AssetUIDs.Any())
                        {
                            var assetDevices = await this._assetDeviceRepository.Fetch(string.Join(",", request.AssetUIDs.Select(assetUID => "UNHEX('" + assetUID + "')").ToArray()));

                            if (assetDevices != null && assetDevices.Count() > 0)
                            {
                                foreach (var assetDevice in assetDevices)
                                {
                                    //CatVIMS check
                                    if (request.HoursMeter != null && request.HoursMeter.ProposedValue.HasValue && request.HoursMeter.CurrentValue.HasValue && (request.HoursMeter.ProposedValue != request.HoursMeter.CurrentValue && deviceConfigurationValues.Configurations.MetersConfig.NotAllowedModuleType != null)) // for both backvalue and updation
                                    {
                                        if (deviceConfigurationValues.Configurations.MetersConfig.NotAllowedModuleType.Contains(assetDevice.ModuleType))
                                        {
                                            hourMeterRemoval = true;
                                            result.Add(base.GetValidationResult(ErrorCodes.MetersUpdationNotAllowedForHoursMeter, Utils.GetEnumDescription(ErrorCodes.MetersUpdationNotAllowedForHoursMeter), false, "MetersValidator.Validate"));
                                        }
                                    }
                                }
                            }
                        }

                        if (hourMeterRemoval)
                        {
                            foreach(var hourmeter in hourMeterKeys)
                            {
                                request.ConfigValues.Remove(hourmeter);
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
