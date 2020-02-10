using AutoMapper;
using ClientModel.DeviceConfig.Request;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig.DeviceTypeGroupParameterAttribute;
using CommonModel.Error;
using CommonModel.Exceptions;
using Infrastructure.Cache.Interfaces;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Logging;

namespace Infrastructure.Service.DeviceConfig.Implementations
{
	public abstract class DeviceConfigServiceBase
    {
        protected readonly IMapper _mapper;
        protected readonly IParameterAttributeCache _parameterAttributeCache;
        protected readonly ILoggingService _loggingService;
		protected IDictionary<string, DeviceTypeGroupParameterAttributeDetails> _paramterAttributeDetails;

        public DeviceConfigServiceBase(IParameterAttributeCache parameterAttributeCache, IMapper mapper,
            ILoggingService loggingService)
        {
            this._parameterAttributeCache = parameterAttributeCache;
            this._mapper = mapper;
            this._loggingService = loggingService;
            this._loggingService.CreateLogger(this.GetType());
			this._paramterAttributeDetails = new Dictionary<string, DeviceTypeGroupParameterAttributeDetails>();
		}

        protected async Task<IList<IErrorInfo>> Validate<T>(IEnumerable<IRequestValidator<T>> validators, T request) where T : IServiceRequest
        {
            this._loggingService.Info("Started validation for the Device Config Settings Request", "DeviceConfigServiceBase.Validate");

            List<IErrorInfo> errorInfos = new List<IErrorInfo>();
            foreach (var validator in validators)
            {
                this._loggingService.Info("Starting validation for validator : " + validator.ToString(), "DeviceConfigServiceBase.Validate");
                var validationResult = await validator.Validate(request);
                this._loggingService.Info("Ended validation for validator : " + validator.ToString(), "DeviceConfigServiceBase.Validate");
                if (validationResult == null)
                {
                    continue;
                }
                this._loggingService.Info("Validation response for validator : " + validator.ToString() + " - " + JsonConvert.SerializeObject(validationResult), "DeviceConfigServiceBase.Validate");
                errorInfos.AddRange(validationResult);
            }

            this._loggingService.Info("Ended validation for the Device Config Settings Request", "DeviceConfigServiceBase.Validate");

            return errorInfos;
        }

        protected virtual void CheckForInvalidRecords(DeviceConfigRequestBase request, List<IErrorInfo> errorInfos, bool assetUIDCheckRequired = true)
        {
            this._loggingService.Info("Started Checking for Invalid Records", "DeviceConfigServiceBase.CheckForInvalidRecords");

            var invalidRecords = errorInfos.Where(x => x.IsInvalid);

            if (errorInfos.Where(x => x.IsInvalid).Any())
            {
                this._loggingService.Info("Ignoring request since following records are invalid : " + JsonConvert.SerializeObject(invalidRecords), "DeviceConfigServiceBase.CheckForInvalidRecords");
                throw new DomainException { Errors = errorInfos };
            }
            if (assetUIDCheckRequired)
            {
                if (request.AssetUIDs == null || !request.AssetUIDs.Any())
                {
                    throw new DomainException
                    {
                        Errors = errorInfos.Any() ? errorInfos : new List<IErrorInfo>
                        {
                            new ErrorInfo
                            {
                                ErrorCode = (int)ErrorCodes.AssetUIDListNull,
                                Message = Utils.GetEnumDescription(ErrorCodes.AssetUIDListNull)
                            }
                        }
                    };
                }
                this._loggingService.Info("Ignoring request since following records are invalid : " + JsonConvert.SerializeObject(invalidRecords), "DeviceConfigServiceBase.CheckForInvalidRecords");
            }
            this._loggingService.Info("Ended Checking for Invalid Records", "DeviceConfigServiceBase.CheckForInvalidRecords");
        }

        protected async virtual Task<IDictionary<string, DeviceTypeGroupParameterAttributeDetails>> GetAttributeIds(string deviceType, string parameterGroupName)
        {
            this._loggingService.Info("Started fetching Attribute Ids from Cache", "DeviceConfigServiceBase.GetAttributeIds");
            try
            {
				if (!_paramterAttributeDetails.Any(x => x.Value.TypeName == deviceType && x.Value.GroupName == parameterGroupName))
				{
					_paramterAttributeDetails.Clear();
					var cachedAttributes = await this._parameterAttributeCache.Get(deviceType, parameterGroupName);
					foreach (var cachedAttribute in cachedAttributes)
					{
						_paramterAttributeDetails.Add(cachedAttribute.ParameterName + "." + cachedAttribute.AttributeName, _mapper.Map<DeviceTypeGroupParameterAttributeDetails>(cachedAttribute));
					}
					this._loggingService.Info("Ended fetching Attribute Ids from Cache", "DeviceConfigServiceBase.GetAttributeIds");
				}
            }
            catch(Exception ex)
            {
                this._loggingService.Error("An unexpected error has occurred", "DeviceConfigServiceBase.GetAttributeIds", ex);
            }
            return _paramterAttributeDetails;
        }
    }
}
