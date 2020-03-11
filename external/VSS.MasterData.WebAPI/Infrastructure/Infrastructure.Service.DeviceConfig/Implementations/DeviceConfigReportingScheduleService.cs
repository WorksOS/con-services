using AutoMapper;
using ClientModel.DeviceConfig.Request;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.ReportingSchedule;
using ClientModel.DeviceConfig.Response.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig.ReportingSchedule;
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
using VSS.MasterData.WebAPI.Transactions;
using Infrastructure.Service.DeviceAcknowledgementByPasser.Interfaces;
using Infrastructure.Service.DeviceMessageConstructor.Interfaces;
using CommonModel.DeviceSettings;
using Infrastructure.Cache.Interfaces;

namespace Infrastructure.Service.DeviceConfig.Implementations
{
	public class DeviceConfigReportingScheduleService : DeviceConfigRepositoryServiceBase, IDeviceConfigService<DeviceConfigReportingScheduleRequest, DeviceConfigReportingScheduleDetails>
    {
        private readonly IEnumerable<IRequestValidator<DeviceConfigRequestBase>> _deviceConfigRequestValidators;
        private readonly IEnumerable<IRequestValidator<IServiceRequest>> _serviceRequestValidators;
        private readonly IEnumerable<IRequestValidator<DeviceConfigReportingScheduleRequest>> _reportingScheduleValidators;

        public DeviceConfigReportingScheduleService(IInjectConfig injectConfig, IDeviceConfigRepository deviceConfigRepository, IEnumerable<IRequestValidator<DeviceConfigRequestBase>> deviceConfigRequestValidators,
            IEnumerable<IRequestValidator<IServiceRequest>> serviceRequestValidators,
            IEnumerable<IRequestValidator<DeviceConfigReportingScheduleRequest>> reportingScheduleValidators,
            IAssetDeviceRepository assetDeviceRepository,
			IOptions<Configurations> configurations,
			IParameterAttributeCache parameterAttributeCache, IMapper mapper, IMessageConstructor messageConstructor, IAckBypasser ackBypasser, IDeviceConfigSettingConfig settingsConfig, ITransactions transactions, ILoggingService loggingService) : base(injectConfig, deviceConfigRepository, parameterAttributeCache, mapper, assetDeviceRepository, messageConstructor,ackBypasser,settingsConfig, configurations, transactions, loggingService)
        {
            this._deviceConfigRequestValidators = deviceConfigRequestValidators;
            this._serviceRequestValidators = serviceRequestValidators;
            this._reportingScheduleValidators = reportingScheduleValidators;
            this._loggingService.CreateLogger(this.GetType());
        }

        public async Task<DeviceConfigServiceResponse<DeviceConfigReportingScheduleDetails>> Fetch(DeviceConfigReportingScheduleRequest request)
        {
            IList<DeviceConfigReportingScheduleDetails> deviceConfigReportingScheduleDetails = new List<DeviceConfigReportingScheduleDetails>();
            IList<DeviceConfigDto> deviceConfigDtos = new List<DeviceConfigDto>();
            List<IErrorInfo> errorInfos = new List<IErrorInfo>();

            errorInfos.AddRange(await base.Validate(this._deviceConfigRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._serviceRequestValidators, request));

            base.CheckForInvalidRecords(request, errorInfos);

            var deviceConfigResponseDtos = await base.Fetch(request);

            if (deviceConfigResponseDtos != null && deviceConfigResponseDtos.Any())
            {
                deviceConfigReportingScheduleDetails = this.BuildResponse<DeviceConfigReportingScheduleRequest, DeviceConfigReportingScheduleDetails>(request, deviceConfigResponseDtos);
            }

            return new DeviceConfigServiceResponse<DeviceConfigReportingScheduleDetails>(deviceConfigReportingScheduleDetails, errorInfos);
        }

        public async Task<DeviceConfigServiceResponse<DeviceConfigReportingScheduleDetails>> Save(DeviceConfigReportingScheduleRequest request)
        {
            IList<DeviceConfigReportingScheduleDetails> deviceConfigReportingScheduleDetails = new List<DeviceConfigReportingScheduleDetails>();
            List<DeviceConfigDto> deviceConfigDtos = new List<DeviceConfigDto>();
            List<IErrorInfo> errorInfos = new List<IErrorInfo>();

            errorInfos.AddRange(await base.Validate(this._deviceConfigRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._serviceRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._reportingScheduleValidators, request));

            base.CheckForInvalidRecords(request, errorInfos);

            var deviceConfigResponseDtos = await base.Save(request);

            if (deviceConfigResponseDtos != null && deviceConfigResponseDtos.Any())
            {
                deviceConfigReportingScheduleDetails = this.BuildResponse<DeviceConfigReportingScheduleRequest, DeviceConfigReportingScheduleDetails>(request, deviceConfigResponseDtos);
            }
            return new DeviceConfigServiceResponse<DeviceConfigReportingScheduleDetails>(deviceConfigReportingScheduleDetails, errorInfos);
        }


        protected override IList<TOut> BuildResponse<TIn, TOut>(TIn request, IList<DeviceConfigDto> deviceConfigDtos)
        {
            List<TOut> deviceConfigServiceResponseDetails = new List<TOut>();
            var attributeIDs = base.GetAttributeIds(request.DeviceType, request.ParameterGroupName).Result;
                
            foreach (var deviceConfigs in deviceConfigDtos.GroupBy(x => x.AssetUIDString))
            {
                DeviceConfigReportingScheduleDetails result = new DeviceConfigReportingScheduleDetails();
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
                        case "DailyReportingTime":
                            TimeSpan dailyReportingTime;
                            result.DailyReportingTime = new ValueWithPendingFlag<TimeSpan?> { IsPending = deviceConfig.IsPending };
                            if (!string.IsNullOrEmpty(deviceConfig.AttributeValue) && TimeSpan.TryParse(deviceConfig.AttributeValue, out dailyReportingTime))
                            {
                                result.DailyReportingTime.Value = dailyReportingTime;
                            }
                            break;
                        case "DailyLocationReportingFrequency":
                            int dailyLocationReportingFrequency;
                            result.DailyLocationReportingFrequency = new ValueWithPendingFlag<int?> { IsPending = deviceConfig.IsPending };
                            if (!string.IsNullOrEmpty(deviceConfig.AttributeValue) && Int32.TryParse(deviceConfig.AttributeValue, out dailyLocationReportingFrequency))
                            {
                                result.DailyLocationReportingFrequency.Value = dailyLocationReportingFrequency;
                            }
                            break;
                        case "HourMeterFuelReport":
                            result.HourMeterFuelReport = new ValueWithPendingFlag<string> { Value = deviceConfig.AttributeValue, IsPending = deviceConfig.IsPending };
                            break;
                        case "ReportAssetStartStop":
                            result.ReportAssetStartStop = new ValueWithPendingFlag<bool?>
                            {
                                IsPending = deviceConfig.IsPending
                            };
                            if (!string.IsNullOrEmpty(deviceConfig.AttributeValue))
                            {
                                result.ReportAssetStartStop.Value = Convert.ToBoolean(deviceConfig.AttributeValue);
                            }
                            break;
                        case "GlobalGram":
                            result.GlobalGram = new ValueWithPendingFlag<bool?>
                            {
                                IsPending = deviceConfig.IsPending
                            };
                            if (!string.IsNullOrEmpty(deviceConfig.AttributeValue))
                            {
                                result.GlobalGram.Value = Convert.ToBoolean(deviceConfig.AttributeValue);
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
