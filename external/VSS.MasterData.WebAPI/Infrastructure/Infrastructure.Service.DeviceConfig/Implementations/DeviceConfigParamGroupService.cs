using AutoMapper;
using ClientModel.DeviceConfig.Request;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.ParameterGroup;
using ClientModel.DeviceConfig.Response.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig.ParameterGroup;
using CommonModel.Error;
using DbModel.DeviceConfig;
using Infrastructure.Cache.Interfaces;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.Logging;

namespace Infrastructure.Service.DeviceConfig.Implementations
{
	public class DeviceConfigParamGroupService : DeviceConfigServiceBase, IDeviceConfigService<DeviceConfigParameterGroupRequest, ParameterGroupDetails>
    {
        private readonly IDeviceParamGroupRepository _deviceConfigParamGroupRepository;
        private readonly IEnumerable<IRequestValidator<DeviceConfigRequestBase>> _deviceConfigBaseValidators;
        private readonly IEnumerable<IRequestValidator<IServiceRequest>> _serviceRequestValidators;
        private const string logMethodFormat = "DeviceConfigParamGroupService.{0}";

        public DeviceConfigParamGroupService(IDeviceParamGroupRepository deviceConfigParamGroupRepository, 
            IEnumerable<IRequestValidator<DeviceConfigRequestBase>> deviceConfigBaseValidators,
            IEnumerable<IRequestValidator<IServiceRequest>> serviceRequestValidators,
            IParameterAttributeCache parameterAttributeCache,
            IMapper mapper, 
            ILoggingService loggingService) 
            : base(parameterAttributeCache, mapper, loggingService)
        {
            this._deviceConfigParamGroupRepository = deviceConfigParamGroupRepository;
            this._deviceConfigBaseValidators = deviceConfigBaseValidators;
            this._serviceRequestValidators = serviceRequestValidators;
            this._loggingService.CreateLogger(typeof(DeviceConfigParamGroupService));
        }

        public async Task<DeviceConfigServiceResponse<ParameterGroupDetails>> Fetch(DeviceConfigParameterGroupRequest request)
        {
            try
            {
                List<IErrorInfo> errorsInfo = new List<IErrorInfo>();

                base._loggingService.Debug("Started executing method", string.Format(logMethodFormat, "FetchDeviceTypeParamGroups"));

                base._loggingService.Debug("Started validation for request : " + JsonConvert.SerializeObject(request), string.Format(logMethodFormat, "FetchDeviceTypeParamGroups"));

                errorsInfo.AddRange(await base.Validate<DeviceConfigRequestBase>(_deviceConfigBaseValidators, request));
                errorsInfo.AddRange(await base.Validate<IServiceRequest>(_serviceRequestValidators, request));

                base.CheckForInvalidRecords(request, errorsInfo, false);

                base._loggingService.Debug("Ended validation", string.Format(logMethodFormat, "FetchDeviceTypeParamGroups"));

                base._loggingService.Info("Device Type Parameter Groups for request : " + JsonConvert.SerializeObject(request), string.Format(logMethodFormat, "FetchDeviceTypeParamGroups"));

                var parameterGroupsDto = await this._deviceConfigParamGroupRepository.FetchDeviceTypeParamGroups(new DeviceParamGroupDto
                {
                    TypeName = request.DeviceType
                });

                var parameterGroupDetails = base._mapper.Map<List<ParameterGroupDetails>>(parameterGroupsDto);

                base._loggingService.Debug("Ended executing method", string.Format(logMethodFormat, "FetchDeviceTypeParamGroups"));

                return new DeviceConfigServiceResponse<ParameterGroupDetails>(parameterGroupDetails, errorsInfo);
            }
            catch(Exception ex)
            {
                this._loggingService.Error("An Exception has occurred", string.Format(logMethodFormat, "FetchDeviceTypeParamGroups"), ex);
                throw ex;
            }
        }

        public Task<DeviceConfigServiceResponse<ParameterGroupDetails>> Save(DeviceConfigParameterGroupRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
