using AutoMapper;
using ClientModel.DeviceConfig.Request;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.AssetSecurity;
using ClientModel.DeviceConfig.Response.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig.Asset_Security;
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
	public class DeviceConfigAssetSecurityService : DeviceConfigRepositoryServiceBase, IDeviceConfigService<DeviceConfigAssetSecurityRequest, DeviceConfigAssetSecurityDetails>
    {
        private readonly IEnumerable<IRequestValidator<DeviceConfigRequestBase>> _deviceConfigRequestValidators;
        private readonly IEnumerable<IRequestValidator<IServiceRequest>> _serviceRequestValidators;
        private readonly IEnumerable<IRequestValidator<DeviceConfigAssetSecurityRequest>> _assetSecurityValidators;
		private readonly IOptions<Configurations> _configurations;

		public DeviceConfigAssetSecurityService(IInjectConfig injectConfig, IDeviceConfigRepository deviceConfigRepository, IEnumerable<IRequestValidator<DeviceConfigRequestBase>> deviceConfigRequestValidators,
            IEnumerable<IRequestValidator<IServiceRequest>> serviceRequestValidators,
            IEnumerable<IRequestValidator<DeviceConfigAssetSecurityRequest>> assetSecurityValidators,
            IAssetDeviceRepository assetDeviceRepository,
			IOptions<Configurations> configurations,
			IParameterAttributeCache parameterAttributeCache, IMapper mapper, IMessageConstructor messageConstructor, IAckBypasser ackBypasser, IDeviceConfigSettingConfig settingsConfig, ITransactions transactions, ILoggingService loggingService) : base(injectConfig, deviceConfigRepository, parameterAttributeCache, mapper, assetDeviceRepository, messageConstructor, ackBypasser, settingsConfig, configurations, transactions, loggingService)
        {
			this._configurations = configurations;
			this._deviceConfigRequestValidators = deviceConfigRequestValidators;
            this._serviceRequestValidators = serviceRequestValidators;
            this._assetSecurityValidators = assetSecurityValidators;
            this._loggingService.CreateLogger(this.GetType());
        }

        public async Task<DeviceConfigServiceResponse<DeviceConfigAssetSecurityDetails>> Fetch(DeviceConfigAssetSecurityRequest request)
        {
            IList<DeviceConfigAssetSecurityDetails> deviceConfigAssetSecurityDetails = new List<DeviceConfigAssetSecurityDetails>();
            IList<DeviceConfigDto> deviceConfigDtos = new List<DeviceConfigDto>();
            List<IErrorInfo> errorInfos = new List<IErrorInfo>();

            errorInfos.AddRange(await base.Validate(this._deviceConfigRequestValidators, request));
            errorInfos.AddRange(await base.Validate(_serviceRequestValidators, request));

            base.CheckForInvalidRecords(request, errorInfos);

            var deviceConfigResponseDtos = await base.Fetch(request);

            if (deviceConfigResponseDtos != null && deviceConfigResponseDtos.Any())
            {
                deviceConfigAssetSecurityDetails = base.BuildResponse<DeviceConfigAssetSecurityRequest, DeviceConfigAssetSecurityDetails>(request, deviceConfigResponseDtos);
            }

            return new DeviceConfigServiceResponse<DeviceConfigAssetSecurityDetails>(deviceConfigAssetSecurityDetails, errorInfos);
        }

        public async Task<DeviceConfigServiceResponse<DeviceConfigAssetSecurityDetails>> Save(DeviceConfigAssetSecurityRequest request)
        {
            IList<DeviceConfigAssetSecurityDetails> deviceConfigAssetSecurityDetails = new List<DeviceConfigAssetSecurityDetails>();
            IList<DeviceConfigDto> deviceConfigDtos = new List<DeviceConfigDto>();
            List<IErrorInfo> errorInfos = new List<IErrorInfo>();

            errorInfos.AddRange(await base.Validate(this._deviceConfigRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._serviceRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._assetSecurityValidators, request));

            base.CheckForInvalidRecords(request, errorInfos);

            var deviceConfigResponseDtos = await base.Save(request);

            if (deviceConfigResponseDtos != null && deviceConfigResponseDtos.Any())
            {
                deviceConfigAssetSecurityDetails = base.BuildResponse<DeviceConfigAssetSecurityRequest, DeviceConfigAssetSecurityDetails>(request, deviceConfigResponseDtos);
            }

            return new DeviceConfigServiceResponse<DeviceConfigAssetSecurityDetails>(deviceConfigAssetSecurityDetails, errorInfos);
        }
    }
}
