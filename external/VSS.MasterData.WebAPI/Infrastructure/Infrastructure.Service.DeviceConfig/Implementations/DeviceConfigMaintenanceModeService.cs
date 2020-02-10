using AutoMapper;
using ClientModel.DeviceConfig.Request;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.MaintenanceMode;
using ClientModel.DeviceConfig.Response.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig.MaintenanceMode;
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
	public class DeviceConfigMaintenanceModeService : DeviceConfigRepositoryServiceBase, IDeviceConfigService<DeviceConfigMaintenanceModeRequest, DeviceConfigMaintenanceModeDetails>
    {
        private readonly IEnumerable<IRequestValidator<DeviceConfigRequestBase>> _deviceConfigRequestValidators;
        private readonly IEnumerable<IRequestValidator<IServiceRequest>> _serviceRequestValidators;
        private readonly IEnumerable<IRequestValidator<DeviceConfigMaintenanceModeRequest>> _MaintenanceModeValidators;
		private readonly IOptions<Configurations> _configurations;

		public DeviceConfigMaintenanceModeService(IInjectConfig injectConfig, IDeviceConfigRepository deviceConfigRepository, IEnumerable<IRequestValidator<DeviceConfigRequestBase>> deviceConfigRequestValidators,
            IEnumerable<IRequestValidator<IServiceRequest>> serviceRequestValidators,
            IEnumerable<IRequestValidator<DeviceConfigMaintenanceModeRequest>> MaintenanceModeValidators,
            IAssetDeviceRepository assetDeviceRepository,
			IOptions<Configurations> configurations,
			IParameterAttributeCache parameterAttributeCache, IMapper mapper, IMessageConstructor messageConstructor, IAckBypasser ackBypasser, IDeviceConfigSettingConfig settingsConfig, ITransactions transactions, ILoggingService loggingService) : base(injectConfig, deviceConfigRepository, parameterAttributeCache, mapper, assetDeviceRepository, messageConstructor,ackBypasser,settingsConfig, configurations, transactions, loggingService)
        {
            this._deviceConfigRequestValidators = deviceConfigRequestValidators;
            this._serviceRequestValidators = serviceRequestValidators;
            this._MaintenanceModeValidators = MaintenanceModeValidators;
            this._loggingService.CreateLogger(this.GetType());
        }

        public async Task<DeviceConfigServiceResponse<DeviceConfigMaintenanceModeDetails>> Fetch(DeviceConfigMaintenanceModeRequest request)
        {
            IList<DeviceConfigMaintenanceModeDetails> deviceConfigMaintenanceModeDetails = new List<DeviceConfigMaintenanceModeDetails>();
            IList<DeviceConfigDto> deviceConfigDtos = new List<DeviceConfigDto>();
            List<IErrorInfo> errorInfos = new List<IErrorInfo>();

            errorInfos.AddRange(await base.Validate(this._deviceConfigRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._serviceRequestValidators, request));

            base.CheckForInvalidRecords(request, errorInfos);

            var deviceConfigResponseDtos = await base.Fetch(request);

            if (deviceConfigResponseDtos != null && deviceConfigResponseDtos.Any())
            {
                deviceConfigMaintenanceModeDetails = base.BuildResponse<DeviceConfigMaintenanceModeRequest, DeviceConfigMaintenanceModeDetails>(request, deviceConfigResponseDtos);
            }

            return new DeviceConfigServiceResponse<DeviceConfigMaintenanceModeDetails>(deviceConfigMaintenanceModeDetails, errorInfos);
        }

        public async Task<DeviceConfigServiceResponse<DeviceConfigMaintenanceModeDetails>> Save(DeviceConfigMaintenanceModeRequest request)
        {
            IList<DeviceConfigMaintenanceModeDetails> deviceConfigMaintenanceModeDetails = new List<DeviceConfigMaintenanceModeDetails>();
            IList<DeviceConfigDto> deviceConfigDtos = new List<DeviceConfigDto>();
            List<IErrorInfo> errorInfos = new List<IErrorInfo>();

            errorInfos.AddRange(await base.Validate(this._deviceConfigRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._serviceRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._MaintenanceModeValidators, request));

            base.CheckForInvalidRecords(request, errorInfos);

            var deviceConfigResponseDtos = await base.Save(request);

            if (deviceConfigResponseDtos != null && deviceConfigResponseDtos.Any())
            {
                deviceConfigMaintenanceModeDetails = base.BuildResponse<DeviceConfigMaintenanceModeRequest, DeviceConfigMaintenanceModeDetails>(request, deviceConfigResponseDtos);
            }

            return new DeviceConfigServiceResponse<DeviceConfigMaintenanceModeDetails>(deviceConfigMaintenanceModeDetails, errorInfos);
        }
    }
}
