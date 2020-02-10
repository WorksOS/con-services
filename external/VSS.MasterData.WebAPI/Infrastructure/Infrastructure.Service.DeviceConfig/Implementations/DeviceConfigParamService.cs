using AutoMapper;
using CommonModel.AssetSettings;
using ClientModel.DeviceConfig.Request;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.Parameter;
using ClientModel.DeviceConfig.Response.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig.Parameter;
using CommonModel.Error;
using DbModel.DeviceConfig;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Logging;
using Infrastructure.Cache.Interfaces;

namespace Infrastructure.Service.DeviceConfig.Implementations
{
	public class DeviceConfigParamService : DeviceConfigParmetersServiceBase
    {
        private readonly IDeviceParamRepository _deviceConfigParamRepository;
        private readonly IEnumerable<IRequestValidator<DeviceConfigParameterRequest>> _parameterGroupValidators;
        private readonly IEnumerable<IRequestValidator<DeviceConfigRequestBase>> _deviceConfigBaseValidators;
        private readonly IEnumerable<IRequestValidator<IServiceRequest>> _serviceRequestValidators;
        private const string logMethodFormat = "DeviceConfigParamService.{0}";

        public DeviceConfigParamService(IDeviceParamRepository deviceConfigParamRepository,
            IEnumerable<IRequestValidator<DeviceConfigParameterRequest>> parameterGroupValidators,
            IEnumerable<IRequestValidator<DeviceConfigRequestBase>> deviceConfigBaseValidators,
            IEnumerable<IRequestValidator<IServiceRequest>> serviceRequestValidators,
            IParameterAttributeCache parameterAttributeCache,
            IMapper mapper,
            ILoggingService loggingService) : base(parameterAttributeCache, mapper, loggingService)
            
        {
            this._deviceConfigParamRepository = deviceConfigParamRepository;
            this._parameterGroupValidators = parameterGroupValidators;
            this._deviceConfigBaseValidators = deviceConfigBaseValidators;
            this._serviceRequestValidators = serviceRequestValidators;
            this._loggingService.CreateLogger(typeof(DeviceConfigParamService));
        }

        public async override Task<DeviceConfigServiceResponse<ParameterDetails>> Fetch(DeviceConfigParameterRequest request)
        {
            try
            {
                List<IErrorInfo> errorsInfo = new List<IErrorInfo>();

                base._loggingService.Debug("Started executing method", string.Format(logMethodFormat, "Fetch"));

                base._loggingService.Debug("Started validation for request : " + JsonConvert.SerializeObject(request), string.Format(logMethodFormat, "Fetch"));

                errorsInfo.AddRange(await base.Validate<DeviceConfigParameterRequest>(_parameterGroupValidators, request));
                errorsInfo.AddRange(await base.Validate<DeviceConfigRequestBase>(_deviceConfigBaseValidators, request));
                errorsInfo.AddRange(await base.Validate<IServiceRequest>(_serviceRequestValidators, request));

                base.CheckForInvalidRecords(request, errorsInfo, false);

                base._loggingService.Debug("Ended validation", string.Format(logMethodFormat, "Fetch"));

                base._loggingService.Info("Device Type Parameter for request : " + JsonConvert.SerializeObject(request), string.Format(logMethodFormat, "Fetch"));


                var parametersDto = await this._deviceConfigParamRepository.FetchDeviceTypeParameters(new DeviceParamDto
                {
                    TypeName = request.DeviceType,
                    ParameterGroupId = request.ParameterGroupID,
                    ParameterGroupName = request.ParameterGroupName
                });

                var parameterDetails = base._mapper.Map<List<ParameterDetails>>(parametersDto);

                base._loggingService.Debug("Ended executing method", string.Format(logMethodFormat, "Fetch"));

                if (request.ParameterGroupID == 4)
                {
                    var cacheParameter = await _parameterAttributeCache.Get(request.DeviceType);
                    parameterDetails = new List<ParameterDetails>();
                    parametersDto.ToList()
                        .ForEach(dto =>
                    {
                        DeviceConfigurationDefaultsAndConfigurations switchParams = JsonConvert.DeserializeObject<DeviceConfigurationDefaultsAndConfigurations>(cacheParameter.First(param => String.Compare(dto.Name, param.ParameterName, StringComparison.OrdinalIgnoreCase) == 0).DefaultValueJSON);
                        if (!parameterDetails.Any(det => det.Id == dto.Id) && !switchParams.Configurations.SwitchesConfig.isTampered)
                        {
                            parameterDetails.Add(_mapper.Map<ParameterDetails>(dto));
                        }
                    });

                    parameterDetails.ForEach(paramDet =>
                    {
                        DeviceConfigurationDefaultsAndConfigurations switchParams = JsonConvert.DeserializeObject<DeviceConfigurationDefaultsAndConfigurations>(cacheParameter.First(param => string.Compare(paramDet.Name, param.ParameterName, StringComparison.OrdinalIgnoreCase) == 0).DefaultValueJSON);
                        paramDet.Attributes = parametersDto.Where(paramObj => paramDet.Id == paramObj.Id && paramDet.Name == paramObj.Name)
                                                .Select(paramObj =>
                                                new Attributes
                                                {
                                                    AttributeId = paramObj.AttributeId,
                                                    AttributeName = paramObj.AttributeName,
                                                    DisplayName = paramObj.AttributeName
                                                }).ToArray();
                        paramDet.settings = new Dictionary<string, string>();
                        paramDet.settings.Add("type", switchParams.Configurations.SwitchesConfig.isSingleState ? SwitchType.SingleStateSwitch.ToString() : SwitchType.DualStateSwitch.ToString());
                        paramDet.settings.Add("switchNumber", Convert.ToString(switchParams.Configurations.SwitchesConfig.MaskedSwitchNumber == 0 ? switchParams.Configurations.SwitchesConfig.SwitchNumber : switchParams.Configurations.SwitchesConfig.MaskedSwitchNumber));
                        paramDet.settings.Add("switchPhysicalNumber", Convert.ToString(switchParams.Configurations.SwitchesConfig.SwitchNumber));
                        paramDet.settings.Add("switchLabel", Convert.ToString(switchParams.Configurations.SwitchesConfig.SwitchLabel));
                        if (!switchParams.Configurations.SwitchesConfig.isSingleState)
                            paramDet.settings.Add("switchPowerType", Convert.ToString(switchParams.Configurations.SwitchesConfig.SwitchPowerType));
                    });
                }
                return new DeviceConfigServiceResponse<ParameterDetails>(parameterDetails, errorsInfo);
            }
            catch (Exception ex)
            {
                this._loggingService.Error("An Exception has occurred", string.Format(logMethodFormat, "Fetch"), ex);
                throw ex;
            }
        }

        public async override Task<DeviceConfigServiceResponse<ParameterDetails>> FetchByDeviceType(DeviceConfigParameterRequest request)
        {
            try
            {
                List<IErrorInfo> errorsInfo = new List<IErrorInfo>();

                base._loggingService.Debug("Started executing method", string.Format(logMethodFormat, "Fetch"));

                base._loggingService.Debug("Started validation for request : " + JsonConvert.SerializeObject(request), string.Format(logMethodFormat, "Fetch"));

                errorsInfo.AddRange(await base.Validate<DeviceConfigRequestBase>(_deviceConfigBaseValidators, request));
                errorsInfo.AddRange(await base.Validate<IServiceRequest>(_serviceRequestValidators, request));

                base.CheckForInvalidRecords(request, errorsInfo, false);

                base._loggingService.Debug("Ended validation", string.Format(logMethodFormat, "Fetch"));

                base._loggingService.Info("Device Type Parameter for request : " + JsonConvert.SerializeObject(request), string.Format(logMethodFormat, "Fetch"));


                var parametersDto = await this._deviceConfigParamRepository.FetchDeviceTypeParametersByDeviceType(new DeviceParamDto
                {
                    TypeName = request.DeviceType
                });

                var parameterDetails = base._mapper.Map<List<ParameterDetails>>(parametersDto);

                base._loggingService.Debug("Ended executing method", string.Format(logMethodFormat, "Fetch"));

                return new DeviceConfigServiceResponse<ParameterDetails>(parameterDetails, errorsInfo);
            }
            catch (Exception ex)
            {
                this._loggingService.Error("An Exception has occurred", string.Format(logMethodFormat, "Fetch"), ex);
                throw ex;
            }
        }
        
        public override Task<DeviceConfigServiceResponse<ParameterDetails>> Save(DeviceConfigParameterRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
