using AutoMapper;
using ClientModel.DeviceConfig.Request;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.Switches;
using ClientModel.DeviceConfig.Response.DeviceConfig;
using CommonModel.DeviceSettings;
using CommonModel.Error;
using CommonModel.Exceptions;
using DbModel.DeviceConfig;
using Infrastructure.Cache.Interfaces;
using Infrastructure.Service.DeviceAcknowledgementByPasser.Interfaces;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.IOC;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;

namespace Infrastructure.Service.DeviceConfig.Implementations
{
	public abstract class DeviceConfigTemplateBase<TReq, TRes> : DeviceConfigRepositoryServiceBase, IDeviceConfigService<TReq, TRes>
          where TRes : DeviceConfigResponseBase, new()
          where TReq : DeviceConfigRequestBase
    {

        protected readonly IEnumerable<IRequestValidator<IServiceRequest>> _requestInvalidateValidators;
        protected readonly IEnumerable<IRequestValidator<DeviceConfigRequestBase>> _commonDeviceLevelValidators;
        protected readonly IParameterAttributeCache _parameterAttributeCache;

        protected DeviceConfigTemplateBase(IInjectConfig injectInfo,
            IDeviceConfigRepository deviceConfigRepository,
            IParameterAttributeCache parameterAttributeCache,
            IMapper mapper, ILoggingService loggingService,
            IEnumerable<IRequestValidator<IServiceRequest>> requestInvalidateValidators,
            IEnumerable<IRequestValidator<DeviceConfigRequestBase>> commonDeviceLevelValidators,
            IAssetDeviceRepository assetDeviceRepository,
            IMessageConstructor messageConstructor,IAckBypasser ackBypasser, IOptions<Configurations> configurations, IDeviceConfigSettingConfig settingsConfig, ITransactions transactions) :
            base(injectInfo, deviceConfigRepository, parameterAttributeCache, mapper, assetDeviceRepository, messageConstructor,ackBypasser,settingsConfig, configurations, transactions, loggingService)
        {
            _requestInvalidateValidators = requestInvalidateValidators;
            _commonDeviceLevelValidators = commonDeviceLevelValidators;
            _parameterAttributeCache = parameterAttributeCache;
        }

        protected abstract Task initRequest(DeviceConfigSwitchesRequest request);

        public async Task<DeviceConfigServiceResponse<TRes>> Fetch(TReq request)
        {
            if(request == null)
                
            _loggingService.Info("Template Base Fetch Called", "DeviceConfigTemplateBase.Fetch");
            //var errors = new List<IErrorInfo>();
            //IList<TRes> response = new List<TRes>();

            var errors = new List<IErrorInfo>();
            IList<TRes> response = new List<TRes>();

            errors = await DoValidation(request);

            //errors = await DoValidation(request);

            var deviceConfigDto = await base.Fetch(request);

            response = await GetResponse(request, deviceConfigDto);
            
            _loggingService.Info("Template Base Fetch Call Ended", "DeviceConfigTemplateBase.Fetch");
            
            return new DeviceConfigServiceResponse<TRes>(response, errors);
            
        }

        protected async Task<List<IErrorInfo>> DoValidation(DeviceConfigRequestBase request)
        {
            var errorInfos = new List<IErrorInfo>();
            var _toValidate = _commonDeviceLevelValidators.Where(validators => string.Compare(validators.GetType().Name, "AllAttributeAsMandatoryValidator", StringComparison.OrdinalIgnoreCase) != 0);
            errorInfos.AddRange(await this.Validate(_requestInvalidateValidators, request));
            errorInfos.AddRange(await this.Validate(_toValidate, request));
            base.CheckForInvalidRecords(request, errorInfos);
            return errorInfos;
        }

        protected abstract Task<IList<TRes>> GetResponse(TReq request, IList<DeviceConfigDto> deviceConfigDto);
        protected abstract Task<IList<IErrorInfo>> DoParameterSpecificValidation(TReq request);

        protected bool isAllRecordsInvalid(IList<IErrorInfo> errors, DeviceConfigRequestBase request)
        {
            return errors.All(error => request.AssetUIDs.Contains((error as AssetErrorInfo).AssetUID));
        }
        
        public async Task<DeviceConfigServiceResponse<TRes>> Save(TReq request)
        {
            _loggingService.Info("Template Base Fetch Called", "DeviceConfigTemplateBase.Fetch");
            var errors = new List<IErrorInfo>();
            IList<TRes> response = new List<TRes>();

            errors = await DoValidation(request);

            var validationResult = await DoParameterSpecificValidation(request);

            //errors.AddRange(await reqeustLevelValidators.Validate(request));
            if (validationResult .Count > 0)
            {
                //Process the errors if it is there
                errors.AddRange(validationResult);
                _loggingService.Info(JsonConvert.SerializeObject(errors.Select(error => error as AssetErrorInfo)), "DeviceConfigTemplateBase.Fetch");
                throw new DomainException { Errors = errors };
            }

            await initRequest(request as DeviceConfigSwitchesRequest);

            var configDto = await base.Save(request);
            response = await GetResponse(request, configDto);
            
            return new DeviceConfigServiceResponse<TRes>(response, errors);
        }
    }
}
