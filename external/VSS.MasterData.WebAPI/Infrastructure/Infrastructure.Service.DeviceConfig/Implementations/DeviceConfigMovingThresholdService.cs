using AutoMapper;
using ClientModel.DeviceConfig.Request;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.MovingThresold;
using ClientModel.DeviceConfig.Response.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig.MovingThreshold;
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
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using Infrastructure.Service.DeviceAcknowledgementByPasser.Interfaces;
using VSS.MasterData.WebAPI.Transactions;
using CommonModel.DeviceSettings;
using Infrastructure.Cache.Interfaces;

namespace Infrastructure.Service.DeviceConfig.Implementations
{
	public class DeviceConfigMovingThresholdService : DeviceConfigRepositoryServiceBase, IDeviceConfigService<DeviceConfigMovingThresholdRequest, DeviceConfigMovingThresholdDetails>
    {
        private readonly IEnumerable<IRequestValidator<DeviceConfigRequestBase>> _deviceConfigRequestValidators;
        private readonly IEnumerable<IRequestValidator<DeviceConfigMovingThresholdRequest>> _movingThresholdValidators;
        private readonly IEnumerable<IRequestValidator<IServiceRequest>> _serviceRequestValidators;

        public DeviceConfigMovingThresholdService(IInjectConfig injectConfig, IDeviceConfigRepository deviceConfigRepository, IEnumerable<IRequestValidator<DeviceConfigRequestBase>> deviceConfigRequestValidators, IEnumerable<IRequestValidator<DeviceConfigMovingThresholdRequest>> movingThresholdValidators,
            IEnumerable<IRequestValidator<IServiceRequest>> serviceRequestValidators,
            IAssetDeviceRepository assetDeviceRepository,
			IOptions<Configurations> configurations,
            IParameterAttributeCache parameterAttributeCache, IMapper mapper, IMessageConstructor messageConstructor, IAckBypasser ackBypasser, IDeviceConfigSettingConfig settingsConfig, ITransactions transactions, ILoggingService loggingService) : base(injectConfig, deviceConfigRepository, parameterAttributeCache, mapper, assetDeviceRepository, messageConstructor,ackBypasser,settingsConfig, configurations, transactions, loggingService)
        {
            this._deviceConfigRequestValidators = deviceConfigRequestValidators;
            this._serviceRequestValidators = serviceRequestValidators;
            this._movingThresholdValidators = movingThresholdValidators;
            this._loggingService.CreateLogger(this.GetType());
        }

        public async Task<DeviceConfigServiceResponse<DeviceConfigMovingThresholdDetails>> Fetch(DeviceConfigMovingThresholdRequest request)
        {
            IList<DeviceConfigMovingThresholdDetails> deviceConfigMovingThresholdDetails = new List<DeviceConfigMovingThresholdDetails>();
            IList<DeviceConfigDto> deviceConfigDtos = new List<DeviceConfigDto>();
            List<IErrorInfo> errorInfos = new List<IErrorInfo>();

            errorInfos.AddRange(await base.Validate(this._deviceConfigRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._serviceRequestValidators, request));

            base.CheckForInvalidRecords(request, errorInfos);

            var deviceConfigResponseDtos = await base.Fetch(request);

            if (deviceConfigResponseDtos != null && deviceConfigResponseDtos.Any())
            {
                deviceConfigMovingThresholdDetails = base.BuildResponse<DeviceConfigMovingThresholdRequest, DeviceConfigMovingThresholdDetails>(request, deviceConfigResponseDtos);
            }

            return new DeviceConfigServiceResponse<DeviceConfigMovingThresholdDetails>(deviceConfigMovingThresholdDetails, errorInfos);
        }

        public async Task<DeviceConfigServiceResponse<DeviceConfigMovingThresholdDetails>> Save(DeviceConfigMovingThresholdRequest request)
        {
            IList<DeviceConfigMovingThresholdDetails> deviceConfigMovingThresholdDetails = new List<DeviceConfigMovingThresholdDetails>();
            List<DeviceConfigDto> deviceConfigDtos = new List<DeviceConfigDto>();
            List<IErrorInfo> errorInfos = new List<IErrorInfo>();

            errorInfos.AddRange(await base.Validate(this._deviceConfigRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._serviceRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._movingThresholdValidators, request));

            base.CheckForInvalidRecords(request, errorInfos);

            var deviceConfigResponseDtos = await base.Save(request);

            if (deviceConfigResponseDtos != null && deviceConfigResponseDtos.Any())
            {
                deviceConfigMovingThresholdDetails = base.BuildResponse<DeviceConfigMovingThresholdRequest, DeviceConfigMovingThresholdDetails>(request, deviceConfigResponseDtos);
            }
            return new DeviceConfigServiceResponse<DeviceConfigMovingThresholdDetails>(deviceConfigMovingThresholdDetails, errorInfos);
        }
    }
}
