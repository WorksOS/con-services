using AutoMapper;
using ClientModel.DeviceConfig.Request.DeviceConfig.Parameter;
using ClientModel.DeviceConfig.Response.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig.Parameter;
using Infrastructure.Cache.Interfaces;
using Infrastructure.Service.DeviceConfig.Interfaces;
using System.Threading.Tasks;
using Utilities.Logging;

namespace Infrastructure.Service.DeviceConfig.Implementations
{
	public abstract class DeviceConfigParmetersServiceBase : DeviceConfigServiceBase, IDeviceConfigService<DeviceConfigParameterRequest, ParameterDetails>
    {

        public DeviceConfigParmetersServiceBase(
            IParameterAttributeCache parameterAttributeCache,
            IMapper mapper,
            ILoggingService loggingService) : base(parameterAttributeCache, mapper, loggingService)
        {

        }
        public abstract Task<DeviceConfigServiceResponse<ParameterDetails>> Fetch(DeviceConfigParameterRequest request);

        public abstract Task<DeviceConfigServiceResponse<ParameterDetails>> Save(DeviceConfigParameterRequest request);

        public abstract Task<DeviceConfigServiceResponse<ParameterDetails>> FetchByDeviceType(DeviceConfigParameterRequest request);
    }
}
