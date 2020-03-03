using AutoMapper;
using ClientModel.DeviceConfig.Request;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using ClientModel.DeviceConfig.Request.DeviceConfig.Meters;
using ClientModel.DeviceConfig.Response.DeviceConfig;
using ClientModel.DeviceConfig.Response.DeviceConfig.Meters;
using CommonModel.Error;
using DbModel.DeviceConfig;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
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
	public class DeviceConfigMetersService : DeviceConfigRepositoryServiceBase, IDeviceConfigService<DeviceConfigMetersRequest, DeviceConfigMetersDetails>
    {
        private readonly IEnumerable<IRequestValidator<DeviceConfigRequestBase>> _deviceConfigRequestValidators;
        private readonly IEnumerable<IRequestValidator<IServiceRequest>> _serviceRequestValidators;
        private readonly IEnumerable<IRequestValidator<DeviceConfigMetersRequest>> _metersValidators;
        private readonly IAssetDeviceRepository _assetDeviceRepository;

		public DeviceConfigMetersService(IInjectConfig injectConfig, IDeviceConfigRepository deviceConfigRepository, IEnumerable<IRequestValidator<DeviceConfigRequestBase>> deviceConfigRequestValidators,
            IEnumerable<IRequestValidator<IServiceRequest>> serviceRequestValidators,
            IEnumerable<IRequestValidator<DeviceConfigMetersRequest>> metersValidators,
            IAssetDeviceRepository assetDeviceRepository,
			IOptions<Configurations> configurations,
			IParameterAttributeCache parameterAttributeCache, IMapper mapper, IMessageConstructor messageConstructor, IAckBypasser ackBypasser, IDeviceConfigSettingConfig settingsConfig, ITransactions transactions, ILoggingService loggingService) : base(injectConfig, deviceConfigRepository, parameterAttributeCache, mapper, assetDeviceRepository, messageConstructor, ackBypasser, settingsConfig, configurations, transactions, loggingService)
        {
			this._deviceConfigRequestValidators = deviceConfigRequestValidators;
            this._serviceRequestValidators = serviceRequestValidators;
            this._metersValidators = metersValidators;
            this._assetDeviceRepository = assetDeviceRepository;
            this._loggingService.CreateLogger(this.GetType());
        }

        public async Task<DeviceConfigServiceResponse<DeviceConfigMetersDetails>> Fetch(DeviceConfigMetersRequest request)
        {
            IList<DeviceConfigMetersDetails> deviceConfigMetersDetails = new List<DeviceConfigMetersDetails>();
            IList<DeviceConfigDto> deviceConfigDtos = new List<DeviceConfigDto>();
            List<IErrorInfo> errorInfos = new List<IErrorInfo>();

            errorInfos.AddRange(await base.Validate(this._deviceConfigRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._serviceRequestValidators, request));

            base.CheckForInvalidRecords(request, errorInfos);

            var deviceConfigResponseDtos = await base.Fetch(request);

            if (deviceConfigResponseDtos != null)
            {
                if (!deviceConfigResponseDtos.Any())
                {
                    foreach (var assetUID in request.AssetUIDs) {
                        deviceConfigResponseDtos.Add(new DeviceConfigDto
                        {
                            AssetUIDString = assetUID
                        });
                    };
                }

                deviceConfigMetersDetails = this.BuildResponse<DeviceConfigMetersRequest, DeviceConfigMetersDetails>(request, deviceConfigResponseDtos);
            }

            return new DeviceConfigServiceResponse<DeviceConfigMetersDetails>(deviceConfigMetersDetails, errorInfos);
        }

        public async Task<DeviceConfigServiceResponse<DeviceConfigMetersDetails>> Save(DeviceConfigMetersRequest request)
        {
            IList<DeviceConfigMetersDetails> deviceConfigMetersDetails = new List<DeviceConfigMetersDetails>();
            IList<DeviceConfigDto> deviceConfigDtos = new List<DeviceConfigDto>();
            List<IErrorInfo> errorInfos = new List<IErrorInfo>();

            errorInfos.AddRange(await base.Validate(this._deviceConfigRequestValidators, request));
            errorInfos.AddRange(await base.Validate(this._serviceRequestValidators, request));
            base.CheckForInvalidRecords(request, errorInfos); 
            errorInfos.AddRange(await base.Validate(this._metersValidators, request));


            base.CheckForInvalidRecords(request, errorInfos);

            var deviceConfigResponseDtos = await base.Save(request);

            if (deviceConfigResponseDtos != null && deviceConfigResponseDtos.Any())
            {
                deviceConfigMetersDetails = this.BuildResponse<DeviceConfigMetersRequest, DeviceConfigMetersDetails>(request, deviceConfigResponseDtos);
            }

            return new DeviceConfigServiceResponse<DeviceConfigMetersDetails>(deviceConfigMetersDetails, errorInfos);
        }


        protected override IList<TOut> BuildResponse<TIn, TOut>(TIn request, IList<DeviceConfigDto> deviceConfigDtos)
        {
            this._loggingService.Info("Building Response objects from DeviceConfigDtos", "DeviceConfigMetersService.BuildResponse");

            List<TOut> deviceConfigServiceResponseDetails = new List<TOut>();

            var cachedParameterAttributes = this._parameterAttributeCache.Get(request.DeviceType, request.ParameterGroupName).Result;

            var assetDevices = this._assetDeviceRepository.Fetch(string.Join(",", request.AssetUIDs.Select(assetUID => "UNHEX('" + assetUID + "')").ToArray())).Result;

            var hoursMeter = cachedParameterAttributes.FirstOrDefault(x => x.AttributeName == "HoursMeter");
            var odometer = cachedParameterAttributes.FirstOrDefault(x => x.AttributeName == "Odometer");
            var smhOdometerConfig = cachedParameterAttributes.FirstOrDefault(x => x.AttributeName == "SMHOdometerConfig");

            foreach (var deviceConfigs in deviceConfigDtos.GroupBy(x => x.AssetUIDString))
            {
                DeviceConfigMetersDetails result = new DeviceConfigMetersDetails();
                DeviceConfigDto configDto = null;
                DeviceConfigurationDefaultsAndConfigurations hourMeterDeserializedObject = null;
                DeviceConfigurationDefaultsAndConfigurations odoMeterDeserializedObject = null;

                var metersRequest = request as DeviceConfigMetersRequest;
                if (odometer != null)
                {
                    result.OdoMeter = new MeterModel();
                    result.OdoMeter.CanUpdate = true;
                }

                if (hoursMeter != null)
                {
                    result.HoursMeter = new MeterModel();
                    result.HoursMeter.CanUpdate = true;
                }


                if (hoursMeter != null && !string.IsNullOrEmpty(hoursMeter.DefaultValueJSON))
                {
                    hourMeterDeserializedObject = JsonConvert.DeserializeObject<DeviceConfigurationDefaultsAndConfigurations>(hoursMeter.DefaultValueJSON);
                }

                if (odometer != null && !string.IsNullOrEmpty(odometer.DefaultValueJSON))
                {
                    odoMeterDeserializedObject = JsonConvert.DeserializeObject<DeviceConfigurationDefaultsAndConfigurations>(odometer.DefaultValueJSON);
                }

                if (odoMeterDeserializedObject != null && odoMeterDeserializedObject.Configurations != null)
                {
                    if (result.OdoMeter != null)
                    {
                        result.OdoMeter.AllowBackward = odoMeterDeserializedObject.Configurations.MetersConfig.AllowBackward;
                    }
                }

                if (hourMeterDeserializedObject != null && hourMeterDeserializedObject.Configurations != null)
                {
                    if (result.HoursMeter != null)
                    {
                        result.HoursMeter.AllowBackward = hourMeterDeserializedObject.Configurations.MetersConfig.AllowBackward;
                    }
                }

                if (smhOdometerConfig != null)
                {
                    if (deviceConfigs.Any(x => x.AttributeName == "SMHOdometerConfig" && x.AttributeValue == MetersSmhOdometerConfig.J1939.ToString()))
                    {
                        if (result.HoursMeter != null)
                        {
                            result.HoursMeter.CanUpdate = false;
                        }
                    }
                }

                if (assetDevices != null && assetDevices.Count() > 0)
                {
					var device = assetDevices.FirstOrDefault(x => x.AssetUID == deviceConfigs.FirstOrDefault().AssetUID && string.Compare(x.DeviceType, request.DeviceType) == 0);
					//CatVIMS check

					if (device != null &&
						hourMeterDeserializedObject != null &&
						hourMeterDeserializedObject.Configurations != null &&
                        hourMeterDeserializedObject.Configurations.MetersConfig != null &&
                        hourMeterDeserializedObject.Configurations.MetersConfig.NotAllowedModuleType != null &&
                        hourMeterDeserializedObject.Configurations.MetersConfig.NotAllowedModuleType.Contains(device.ModuleType))
                    {
                        if (result.HoursMeter != null)
                        { 
                            result.HoursMeter.CanUpdate = false;
                        }
                    }
                }

                foreach (var deviceConfig in deviceConfigs)
                {
                    switch (deviceConfig.AttributeName)
                    {
                        case "SMHOdometerConfig":
                            result.SmhOdometerConfig = deviceConfig.AttributeValue;
                            break;
                        case "Odometer":
                            if (!string.IsNullOrEmpty(deviceConfig.AttributeValue))
                            {
                                result.OdoMeter.Value = Convert.ToDouble(deviceConfig.AttributeValue);
                            }
                            break;
                        case "HoursMeter":
                            if (!string.IsNullOrEmpty(deviceConfig.AttributeValue))
                            {
                                result.HoursMeter.Value = Convert.ToDouble(deviceConfig.AttributeValue);
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
