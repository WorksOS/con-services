using AutoMapper;
using CommonModel.DeviceSettings;
using ClientModel.DeviceConfig.Request;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.FaultCodeReporting;
using ClientModel.DeviceConfig.Response.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig.FaultCodeReporting;
using CommonModel.Error;
using DbModel.DeviceConfig;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.IOC;
using Utilities.Logging;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using Infrastructure.Service.DeviceAcknowledgementByPasser.Interfaces;
using VSS.MasterData.WebAPI.Transactions;
using Infrastructure.Cache.Interfaces;

namespace Infrastructure.Service.DeviceConfig.Implementations
{
	public class DeviceConfigFaultCodeReportingService : DeviceConfigRepositoryServiceBase, IDeviceConfigService<DeviceConfigFaultCodeReportingRequest, DeviceConfigFaultCodeReportingDetails>
    {
        private readonly IEnumerable<IRequestValidator<DeviceConfigRequestBase>> _deviceConfigRequestValidators;
        private readonly IEnumerable<IRequestValidator<IServiceRequest>> _serviceRequestValidators;
        private readonly IEnumerable<IRequestValidator<DeviceConfigFaultCodeReportingRequest>> _faultCodeReportingValidators;
		private readonly IOptions<Configurations> _configurations;

		public DeviceConfigFaultCodeReportingService(IInjectConfig injectConfig, IDeviceConfigRepository deviceConfigRepository, IEnumerable<IRequestValidator<DeviceConfigRequestBase>> deviceConfigRequestValidators,
            IEnumerable<IRequestValidator<IServiceRequest>> serviceRequestValidators,
            IEnumerable<IRequestValidator<DeviceConfigFaultCodeReportingRequest>> faultCodeReportingValidators,
            IAssetDeviceRepository assetDeviceRepository,
			IOptions<Configurations> configurations,
			IParameterAttributeCache parameterAttributeCache, IMapper mapper, IMessageConstructor messageConstructor, IAckBypasser ackBypasser, IDeviceConfigSettingConfig settingsConfig, ITransactions transactions, ILoggingService loggingService) : base(injectConfig, deviceConfigRepository, parameterAttributeCache, mapper, assetDeviceRepository, messageConstructor,ackBypasser,settingsConfig, configurations, transactions, loggingService)
        {
            this._deviceConfigRequestValidators = deviceConfigRequestValidators;
            this._serviceRequestValidators = serviceRequestValidators;
            this._faultCodeReportingValidators = faultCodeReportingValidators;
            this._loggingService.CreateLogger(this.GetType());
        }

        public async Task<DeviceConfigServiceResponse<DeviceConfigFaultCodeReportingDetails>> Fetch(DeviceConfigFaultCodeReportingRequest request)
        {
            IList<DeviceConfigFaultCodeReportingDetails> deviceConfigFaultCodeReportingDetails = new List<DeviceConfigFaultCodeReportingDetails>();
            IList<DeviceConfigDto> deviceConfigDtos = new List<DeviceConfigDto>();
            List<IErrorInfo> errorInfos = new List<IErrorInfo>();

            errorInfos.AddRange(await base.Validate(this._deviceConfigRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._serviceRequestValidators, request));

            base.CheckForInvalidRecords(request, errorInfos);

            var deviceConfigResponseDtos = await base.Fetch(request);

            if (deviceConfigResponseDtos != null && deviceConfigResponseDtos.Any())
            {
                deviceConfigFaultCodeReportingDetails = this.BuildResponse<DeviceConfigFaultCodeReportingRequest, DeviceConfigFaultCodeReportingDetails>(request, deviceConfigResponseDtos);
            }

            return new DeviceConfigServiceResponse<DeviceConfigFaultCodeReportingDetails>(deviceConfigFaultCodeReportingDetails, errorInfos);
        }

        public async Task<DeviceConfigServiceResponse<DeviceConfigFaultCodeReportingDetails>> Save(DeviceConfigFaultCodeReportingRequest request)
        {
            IList<DeviceConfigFaultCodeReportingDetails> deviceConfigFaultCodeReportingDetails = new List<DeviceConfigFaultCodeReportingDetails>();
            IList<DeviceConfigDto> deviceConfigDtos = new List<DeviceConfigDto>();
            List<IErrorInfo> errorInfos = new List<IErrorInfo>();

            errorInfos.AddRange(await base.Validate(this._deviceConfigRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._serviceRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._faultCodeReportingValidators, request));

            base.CheckForInvalidRecords(request, errorInfos);

            var deviceConfigResponseDtos = await base.Save(request);

            if (deviceConfigResponseDtos != null && deviceConfigResponseDtos.Any())
            {
                deviceConfigFaultCodeReportingDetails = this.BuildResponse<DeviceConfigFaultCodeReportingRequest, DeviceConfigFaultCodeReportingDetails>(request, deviceConfigResponseDtos);
            }

            return new DeviceConfigServiceResponse<DeviceConfigFaultCodeReportingDetails>(deviceConfigFaultCodeReportingDetails, errorInfos);
        }

        protected override IList<TOut> BuildResponse<TIn, TOut>(TIn request, IList<DeviceConfigDto> deviceConfigDtos)
        {
            List<TOut> deviceConfigServiceResponseDetails = new List<TOut>();
            var attributeIDs = base.GetAttributeIds(request.DeviceType, request.ParameterGroupName).Result;

            foreach (var deviceConfigs in deviceConfigDtos.GroupBy(x => x.AssetUIDString))
            {
                DeviceConfigFaultCodeReportingDetails result = new DeviceConfigFaultCodeReportingDetails();
                DeviceConfigDto configDto = null;
                var deviceConfigLists = deviceConfigs.ToList();

                var attributesNotAvailable = attributeIDs.Keys.Except(deviceConfigLists.Select(y => y.AttributeName));

                if (attributesNotAvailable != null && attributesNotAvailable.Any())
                {
                    deviceConfigLists.AddRange(attributesNotAvailable.Select(x => new DeviceConfigDto
                    {
                        AttributeName = x
                    }));
                }

                foreach (var deviceConfig in deviceConfigLists)
                {
                    switch (deviceConfig.AttributeName)
                    {
                        case "LowSeverityEvents":
                            result.LowSeverityEvents = new ValueWithPendingFlag<string> { Value = deviceConfig.AttributeValue, IsPending = deviceConfig.IsPending };
                            break;
                        case "MediumSeverityEvents":
                            result.MediumSeverityEvents = new ValueWithPendingFlag<string> { Value = deviceConfig.AttributeValue, IsPending = deviceConfig.IsPending };
                            break;
                        case "HighSeverityEvents":
                            result.HighSeverityEvents = new ValueWithPendingFlag<string> { Value = deviceConfig.AttributeValue, IsPending = deviceConfig.IsPending };
                            break;
                        case "DiagnosticReportFrequency":
                            result.DiagnosticReportFrequency = new ValueWithPendingFlag<string> { Value = deviceConfig.AttributeValue, IsPending = deviceConfig.IsPending };
                            break;
                        case "NextSentEventInHours":
                            result.NextSentEventInHours = new ValueWithPendingFlag<int?>
                            {
                                IsPending = deviceConfig.IsPending 
                            };
                            if (!string.IsNullOrEmpty(deviceConfig.AttributeValue))
                            {
                                result.NextSentEventInHours.Value = Convert.ToInt32(deviceConfig.AttributeValue);
                            }
                            break;
                        case "EventDiagnosticFilterInterval":
                            result.EventDiagnosticFilterInterval = new ValueWithPendingFlag<int?>
                            {
                                IsPending = deviceConfig.IsPending
                            };
                            if (!string.IsNullOrEmpty(deviceConfig.AttributeValue))
                            {
                                result.EventDiagnosticFilterInterval.Value = Convert.ToInt32(deviceConfig.AttributeValue);
                            }
                            break;
                    }
                    if (configDto == null)
                    {
                        configDto = deviceConfig;
                    }
                }
                if (configDto != null)
                {
                    result.AssetUID = Guid.Parse(configDto.AssetUIDString);
                    result.LastUpdatedOn = Convert.ToDateTime(configDto.UpdateUTC);
                }
                deviceConfigServiceResponseDetails.Add(result as TOut);
            }
            return deviceConfigServiceResponseDetails;
        }
    }
}
