using ClientModel.DeviceConfig.Request.DeviceConfig.Switches;
using CommonModel.DeviceSettings;
using CommonModel.DeviceSettings.ConfigNameValues;
using CommonModel.Error;
using Infrastructure.Cache.Interfaces;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.IOC;
using Utilities.Logging;

namespace Infrastructure.Service.DeviceConfig.Validators
{
	public class SwitchesValidator : RequestValidatorBase, ISwitchesValidator
    {
        private IParameterAttributeCache _attrCache;
        private ConfigNameValueCollection _attributeMaps;
        private IInjectConfig _injectConfig;
        public SwitchesValidator(ILoggingService loggingService, IParameterAttributeCache attributeCache, IInjectConfig injectConfig) : base(loggingService)
        {
            _attrCache = attributeCache;
            this._injectConfig = injectConfig;
            _attributeMaps = injectConfig.ResolveKeyed<DeviceConfigRequestToAttributeMaps>("DeviceConfigRequestToAttributeMaps");
        }
        public async Task<IList<IErrorInfo>> Validate(DeviceConfigSwitchesRequest request)
        {
            IList<IErrorInfo> errors = new List<IErrorInfo>();
            SwitchActiveState activeState;
            const string InvalidStringValue = "$#$#$";
            SwitchMonitoringStatus switchmonitoringStatus;
            var cachedParamValues = await _attrCache.Get(request.DeviceType, request.ParameterGroupName);

            if (request.SingleStateSwitches != null)
            {
                foreach (var singleStateSwitch in request.SingleStateSwitches)
                {
                    var switchParams = JsonConvert.DeserializeObject<DeviceConfigurationDefaultsAndConfigurations>(cachedParamValues.First(param => string.Compare(singleStateSwitch.SwitchParameterName, param.ParameterName, true) == 0).DefaultValueJSON);
                    if (string.IsNullOrEmpty(singleStateSwitch.SwitchName))
                        errors.Add(GetValidationResult(ErrorCodes.SwitchNameNull, string.Format(Utils.GetEnumDescription(ErrorCodes.SwitchNameNull), singleStateSwitch.SwitchParameterName), true, MethodBase.GetCurrentMethod().Name));
                    if (!string.IsNullOrEmpty(singleStateSwitch.SwitchName) && singleStateSwitch.SwitchName.Length > 64)
                        errors.Add(GetValidationResult(ErrorCodes.SwitchNameInvalid, string.Format(Utils.GetEnumDescription(ErrorCodes.SwitchNameInvalid), singleStateSwitch.SwitchParameterName), true, MethodBase.GetCurrentMethod().Name));
                    if (string.IsNullOrEmpty(Convert.ToString(singleStateSwitch.SwitchSensitivity)))
                        errors.Add(GetValidationResult(ErrorCodes.SwitchSensitivityNull, string.Format(Utils.GetEnumDescription(ErrorCodes.SwitchSensitivityNull), singleStateSwitch.SwitchParameterName), true, MethodBase.GetCurrentMethod().Name));
                    if (singleStateSwitch.SwitchSensitivity > 6550 || singleStateSwitch.SwitchSensitivity < 0)
                        errors.Add(GetValidationResult(ErrorCodes.SwitchSensitivityInvalid, string.Format(Utils.GetEnumDescription(ErrorCodes.SwitchSensitivityInvalid), singleStateSwitch.SwitchParameterName), true, MethodBase.GetCurrentMethod().Name));
                    if (!string.IsNullOrEmpty(singleStateSwitch.SwitchActiveState) && !(Enum.TryParse(singleStateSwitch.SwitchActiveState, out activeState)))
                        errors.Add(GetValidationResult(ErrorCodes.SwitchActiveStatusInvalid, string.Format(Utils.GetEnumDescription(ErrorCodes.SwitchActiveStatusInvalid), singleStateSwitch.SwitchParameterName), true, MethodBase.GetCurrentMethod().Name));
                    if (!string.IsNullOrEmpty(singleStateSwitch.SwitchMonitoringStatus) && !(Enum.TryParse(singleStateSwitch.SwitchMonitoringStatus, out switchmonitoringStatus)))
                        errors.Add(GetValidationResult(ErrorCodes.SwitchMonitoringStatusInvalid, string.Format(Utils.GetEnumDescription(ErrorCodes.SwitchMonitoringStatusInvalid), singleStateSwitch.SwitchParameterName), true, MethodBase.GetCurrentMethod().Name));
                    if (singleStateSwitch.SwitchNumber != switchParams.Configurations.SwitchesConfig.SwitchNumber)
                        errors.Add(GetValidationResult(ErrorCodes.SwitchNumberInvalid, string.Format(Utils.GetEnumDescription(ErrorCodes.SwitchNumberInvalid), singleStateSwitch.SwitchParameterName), true, MethodBase.GetCurrentMethod().Name));
                    if (!cachedParamValues.Any(paramValue => string.Compare(paramValue.ParameterName, singleStateSwitch.SwitchParameterName) == 0))
                        errors.Add(GetValidationResult(ErrorCodes.InvalidParameters, Utils.GetEnumDescription(ErrorCodes.InvalidParameters), true, MethodBase.GetCurrentMethod().Name));

                    var propNames = singleStateSwitch.GetType().GetProperties().Select(props => props.Name);
                    var attributeNames = propNames.Distinct();
                    var requestAttributesId = _attributeMaps.Values.Where(maps => attributeNames.Contains(maps.Value)).ToDictionary(key=>key.Key);

                    var paramAttrId = cachedParamValues.Where(paramValue => string.Compare(paramValue.ParameterName, singleStateSwitch.SwitchParameterName) == 0);
                    
                    var uniqueAttributeIdsForDevice = paramAttrId.Select(paramValues => paramValues.AttributeName.ToString()).Distinct();
                    if (uniqueAttributeIdsForDevice.Any(attrID => !requestAttributesId.ContainsKey(attrID)))
                        errors.Add(GetValidationResult(ErrorCodes.InvalidAttributes, Utils.GetEnumDescription(ErrorCodes.InvalidAttributes), true, MethodBase.GetCurrentMethod().Name));
                }
            }
            if (request.DualStateSwitches != null)
            {
                foreach (var dualStateSwitch in request.DualStateSwitches)
                {
                    DeviceConfigurationDefaultsAndConfigurations switchParams = JsonConvert.DeserializeObject<DeviceConfigurationDefaultsAndConfigurations>(cachedParamValues.First(param => string.Compare(dualStateSwitch.SwitchParameterName, param.ParameterName, true) == 0).DefaultValueJSON);
                    if (!Convert.ToBoolean(dualStateSwitch.SwitchEnabled))
                        continue;
                    if (dualStateSwitch.SwitchNumber != switchParams.Configurations.SwitchesConfig.SwitchNumber || dualStateSwitch.SwitchNumber == 0)
                        errors.Add(GetValidationResult(ErrorCodes.SwitchNumberInvalid, string.Format(Utils.GetEnumDescription(ErrorCodes.SwitchNumberInvalid), dualStateSwitch.SwitchParameterName), true, MethodBase.GetCurrentMethod().Name));
                    if (string.IsNullOrEmpty(dualStateSwitch.SwitchName))
                        errors.Add(GetValidationResult(ErrorCodes.SwitchNameNull, string.Format(Utils.GetEnumDescription(ErrorCodes.SwitchNameNull), dualStateSwitch.SwitchParameterName), true, MethodBase.GetCurrentMethod().Name));
                    if (!string.IsNullOrEmpty(dualStateSwitch.SwitchName) && dualStateSwitch.SwitchName.Length > 64)
                        errors.Add(GetValidationResult(ErrorCodes.SwitchNameInvalid, string.Format(Utils.GetEnumDescription(ErrorCodes.SwitchNameInvalid), dualStateSwitch.SwitchParameterName), true, MethodBase.GetCurrentMethod().Name));
                    if (string.IsNullOrEmpty(dualStateSwitch.SwitchOpen))
                        errors.Add(GetValidationResult(ErrorCodes.SwitchOpenNull, string.Format(Utils.GetEnumDescription(ErrorCodes.SwitchOpenNull), dualStateSwitch.SwitchParameterName), true, MethodBase.GetCurrentMethod().Name));
                    if (string.IsNullOrEmpty(dualStateSwitch.SwitchClosed))
                        errors.Add(GetValidationResult(ErrorCodes.SwitchCloseNull, string.Format(Utils.GetEnumDescription(ErrorCodes.SwitchCloseNull), dualStateSwitch.SwitchParameterName), true, MethodBase.GetCurrentMethod().Name));
                    if (!string.IsNullOrEmpty(dualStateSwitch.SwitchOpen) && dualStateSwitch.SwitchOpen.Length > 64)
                        errors.Add(GetValidationResult(ErrorCodes.SwitchOpenInvalid, string.Format(Utils.GetEnumDescription(ErrorCodes.SwitchOpenInvalid), dualStateSwitch.SwitchParameterName), true, MethodBase.GetCurrentMethod().Name));
                    if (!string.IsNullOrEmpty(dualStateSwitch.SwitchClosed) && dualStateSwitch.SwitchClosed.Length > 64)
                        errors.Add(GetValidationResult(ErrorCodes.SwitchClosedInvalid, string.Format(Utils.GetEnumDescription(ErrorCodes.SwitchClosedInvalid), dualStateSwitch.SwitchParameterName), true, MethodBase.GetCurrentMethod().Name));
                    if (dualStateSwitch.SwitchSensitivity > 6550 || dualStateSwitch.SwitchSensitivity < 0)
                        errors.Add(GetValidationResult(ErrorCodes.SwitchSensitivityDualStateSwitchInvalid, string.Format(Utils.GetEnumDescription(ErrorCodes.SwitchSensitivityDualStateSwitchInvalid), dualStateSwitch.SwitchParameterName), true, MethodBase.GetCurrentMethod().Name));
                    if (!(string.IsNullOrEmpty(dualStateSwitch.SwitchMonitoringStatus) || string.Compare(dualStateSwitch.SwitchMonitoringStatus, InvalidStringValue, StringComparison.OrdinalIgnoreCase) == 0)  && !(Enum.TryParse(dualStateSwitch.SwitchMonitoringStatus, out switchmonitoringStatus)))
                        errors.Add(GetValidationResult(ErrorCodes.SwitchActiveStatusInvalid, string.Format(Utils.GetEnumDescription(ErrorCodes.SwitchActiveStatusInvalid), dualStateSwitch.SwitchParameterName), true, MethodBase.GetCurrentMethod().Name));
                    if (!cachedParamValues.Any(paramValue => string.Compare(paramValue.ParameterName, dualStateSwitch.SwitchParameterName) == 0))
                        errors.Add(GetValidationResult(ErrorCodes.InvalidParameters, Utils.GetEnumDescription(ErrorCodes.InvalidParameters), true, MethodBase.GetCurrentMethod().Name));

                    var propNames = dualStateSwitch.GetType().GetProperties().Select(props => props.Name);
                    var attributeNames = propNames.Distinct();
                    var requestAttributesId = _attributeMaps.Values.Where(maps => attributeNames.Contains(maps.Value)).ToDictionary(key => key.Key);

                    var paramAttrId = cachedParamValues.Where(paramValue => string.Compare(paramValue.ParameterName, dualStateSwitch.SwitchParameterName) == 0);

                    var uniqueAttributeIdsForDevice = paramAttrId.Select(paramValues => paramValues.AttributeName.ToString()).Distinct();
                    if (uniqueAttributeIdsForDevice.Any(attrID => !requestAttributesId.ContainsKey(attrID)))
                        errors.Add(GetValidationResult(ErrorCodes.InvalidAttributes, Utils.GetEnumDescription(ErrorCodes.InvalidAttributes), true, MethodBase.GetCurrentMethod().Name));
                }
            }
            return errors;
        }

        public async Task<IList<IErrorInfo>> ValidateDualStateSwitches(DeviceConfigSwitchesRequest request)
        {
            IList<IErrorInfo> errors = new List<IErrorInfo>();
            var cachedParamValues = await _attrCache.Get(request.DeviceType, request.ParameterGroupName);
            foreach (var cacheParam in cachedParamValues)
            {
                DeviceConfigurationDefaultsAndConfigurations switchParams = JsonConvert.DeserializeObject<DeviceConfigurationDefaultsAndConfigurations>(cacheParam.DefaultValueJSON);
                if (!switchParams.Configurations.SwitchesConfig.isSingleState)
                    return errors;
            }
            errors.Add(GetValidationResult(ErrorCodes.DeviceTypeNotSupported, string.Format(Utils.GetEnumDescription(ErrorCodes.DeviceTypeNotSupported), request.DeviceType), true, MethodBase.GetCurrentMethod().Name));
            return errors;
        }
    }
}