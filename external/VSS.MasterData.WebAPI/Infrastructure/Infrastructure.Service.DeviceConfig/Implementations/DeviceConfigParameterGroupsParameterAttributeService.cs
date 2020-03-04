using AutoMapper;
using ClientModel.DeviceConfig.Request.DeviceConfig.DeviceTypeGroupParameterAttribute;
using ClientModel.DeviceConfig.Response.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig.DeviceTypeGroupParameterAttribute;
using DbModel.DeviceConfig;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.Logging;

namespace Infrastructure.Service.DeviceConfig.Implementations
{
	public class DeviceConfigParameterGroupsParameterAttributeService : IDeviceConfigService<DeviceTypeGroupParameterAttributeRequest, DeviceTypeGroupParameterAttributeDetails>
    {
        private readonly IDeviceTypeGroupParamAttributeRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILoggingService _loggingService;
        private const string logMethodFormat = "DeviceConfigParameterGroupsParameterAttributeService.{0}";

        public DeviceConfigParameterGroupsParameterAttributeService(IDeviceTypeGroupParamAttributeRepository repository,
            IMapper mapper, ILoggingService loggingService)
        {
            this._repository = repository;
            this._mapper = mapper;
            this._loggingService = loggingService;
            this._loggingService.CreateLogger(this.GetType());
        }

        public async Task<DeviceConfigServiceResponse<DeviceTypeGroupParameterAttributeDetails>> Fetch(DeviceTypeGroupParameterAttributeRequest request)
        {
            try
            {
                this._loggingService.Debug("Started executing method", string.Format(logMethodFormat, "Fetch"));

                var parameterAttributesDto = await this._repository.Fetch(new DeviceTypeGroupParamAttrDto());

                var parameterAttributeDetails = this._mapper.Map<List<DeviceTypeGroupParameterAttributeDetails>>(parameterAttributesDto);

                this._loggingService.Debug("Ended executing method", string.Format(logMethodFormat, "Fetch"));

                return new DeviceConfigServiceResponse<DeviceTypeGroupParameterAttributeDetails>(parameterAttributeDetails);
            }
            catch (Exception ex)
            {
                this._loggingService.Error("An Exception has occurred", string.Format(logMethodFormat, "Fetch"), ex);
                throw ex;
            }
        }

        public Task<DeviceConfigServiceResponse<DeviceTypeGroupParameterAttributeDetails>> Save(DeviceTypeGroupParameterAttributeRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
