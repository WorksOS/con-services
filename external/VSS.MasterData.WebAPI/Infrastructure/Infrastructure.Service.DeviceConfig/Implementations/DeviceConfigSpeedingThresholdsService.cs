using AutoMapper;
using ClientModel.DeviceConfig.Request;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.SpeedingThresholds;
using ClientModel.DeviceConfig.Response.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig.SpeedingThresholds;
using CommonModel.Error;
using DbModel.DeviceConfig;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Interfaces;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.IOC;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;
using Infrastructure.Service.DeviceAcknowledgementByPasser.Interfaces;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using CommonModel.DeviceSettings;
using Infrastructure.Cache.Interfaces;

namespace Infrastructure.Service.DeviceConfig.Implementations
{
	public class DeviceConfigSpeedingThresholdsService : DeviceConfigRepositoryServiceBase, IDeviceConfigService<DeviceConfigSpeedingThresholdsRequest, DeviceConfigSpeedingThresholdsDetails>
    {
        private readonly IEnumerable<IRequestValidator<DeviceConfigRequestBase>> _deviceConfigRequestValidators;
        private readonly IEnumerable<IRequestValidator<DeviceConfigSpeedingThresholdsRequest>> _SpeedingThresholdsValidators;
        private readonly IEnumerable<IRequestValidator<IServiceRequest>> _serviceRequestValidators;

        public DeviceConfigSpeedingThresholdsService(IInjectConfig injectConfig, IDeviceConfigRepository deviceConfigRepository, IEnumerable<IRequestValidator<DeviceConfigRequestBase>> deviceConfigRequestValidators, IEnumerable<IRequestValidator<DeviceConfigSpeedingThresholdsRequest>> SpeedingThresholdsValidators,
            IEnumerable<IRequestValidator<IServiceRequest>> serviceRequestValidators,
            IAssetDeviceRepository assetDeviceRepository,
			IOptions<Configurations> configurations,
            IParameterAttributeCache parameterAttributeCache, IMapper mapper, IMessageConstructor messageConstructor, IAckBypasser ackBypasser, IDeviceConfigSettingConfig settingsConfig, ITransactions transactions, ILoggingService loggingService) : base(injectConfig, deviceConfigRepository, parameterAttributeCache, mapper, assetDeviceRepository, messageConstructor, ackBypasser, settingsConfig, configurations, transactions, loggingService)
        {
            this._deviceConfigRequestValidators = deviceConfigRequestValidators;
            this._serviceRequestValidators = serviceRequestValidators;
            this._SpeedingThresholdsValidators = SpeedingThresholdsValidators;
            this._loggingService.CreateLogger(this.GetType());
        }

        public async Task<DeviceConfigServiceResponse<DeviceConfigSpeedingThresholdsDetails>> Fetch(DeviceConfigSpeedingThresholdsRequest request)
        {
            IList<DeviceConfigSpeedingThresholdsDetails> deviceConfigSpeedingThresholdsDetails = new List<DeviceConfigSpeedingThresholdsDetails>();
            IList<DeviceConfigDto> deviceConfigDtos = new List<DeviceConfigDto>();
            List<IErrorInfo> errorInfos = new List<IErrorInfo>();

            errorInfos.AddRange(await base.Validate(this._deviceConfigRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._serviceRequestValidators, request));

            base.CheckForInvalidRecords(request, errorInfos);

            var deviceConfigResponseDtos = await base.Fetch(request);

            if (deviceConfigResponseDtos != null && deviceConfigResponseDtos.Any())
            {
                deviceConfigSpeedingThresholdsDetails = base.BuildResponse<DeviceConfigSpeedingThresholdsRequest, DeviceConfigSpeedingThresholdsDetails>(request, deviceConfigResponseDtos);
            }

            return new DeviceConfigServiceResponse<DeviceConfigSpeedingThresholdsDetails>(deviceConfigSpeedingThresholdsDetails, errorInfos);
        }

        public async Task<DeviceConfigServiceResponse<DeviceConfigSpeedingThresholdsDetails>> Save(DeviceConfigSpeedingThresholdsRequest request)
        {
            IList<DeviceConfigSpeedingThresholdsDetails> deviceConfigSpeedingThresholdsDetails = new List<DeviceConfigSpeedingThresholdsDetails>();
            List<DeviceConfigDto> deviceConfigDtos = new List<DeviceConfigDto>();
            List<IErrorInfo> errorInfos = new List<IErrorInfo>();

            errorInfos.AddRange(await base.Validate(this._deviceConfigRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._serviceRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._SpeedingThresholdsValidators, request));

            base.CheckForInvalidRecords(request, errorInfos);

            var deviceConfigResponseDtos = await base.Save(request);

            if (deviceConfigResponseDtos != null && deviceConfigResponseDtos.Any())
            {
                deviceConfigSpeedingThresholdsDetails = base.BuildResponse<DeviceConfigSpeedingThresholdsRequest, DeviceConfigSpeedingThresholdsDetails>(request, deviceConfigResponseDtos);
            }
            return new DeviceConfigServiceResponse<DeviceConfigSpeedingThresholdsDetails>(deviceConfigSpeedingThresholdsDetails, errorInfos);
        }
    }
}
