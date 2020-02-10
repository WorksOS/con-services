using Interfaces;
using AutoMapper;
using CommonModel.AssetSettings;
using ClientModel.AssetSettings.Request;
using ClientModel.AssetSettings.Response;
using ClientModel.AssetSettings.Response.AssetTargets;
using CommonModel.Error;
using CommonModel.Exceptions;
using ClientModel.Interfaces;
using DbModel.AssetSettings;
using Infrastructure.Cache.Interfaces;
using Infrastructure.Service.AssetSettings.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.Logging;
using DbModel.Cache;

namespace Infrastructure.Service.AssetSettings.Service
{
	public class AssetSettingsListService : IAssetSettingsListService
    {
        private readonly IAssetSettingsListRepository _assetSettingsListRepository;
        private readonly IEnumerable<IRequestValidator<AssetSettingsListRequest>> _assetSettingsListValidators;
        private readonly IEnumerable<IRequestValidator<IServiceRequest>> _serviceRequestParametersValidators;
        private readonly IMapper _mapper;
        private readonly ILoggingService _loggingService;
		private readonly IParameterAttributeCache _parameterAttributeCache;
		private readonly Configurations _configurations;

		public AssetSettingsListService(IAssetSettingsListRepository assetTargetWorkDefinitionData, 
            IEnumerable<IRequestValidator<AssetSettingsListRequest>> assetSettingsListValidators,
            IEnumerable<IRequestValidator<IServiceRequest>> serviceRequestParametersValidators,
			IParameterAttributeCache parameterAttributeCache,
            IMapper mapper,
            ILoggingService loggingService, IOptions<Configurations> configurations)
        {
            this._assetSettingsListValidators = assetSettingsListValidators;
            this._serviceRequestParametersValidators = serviceRequestParametersValidators;
            this._assetSettingsListRepository = assetTargetWorkDefinitionData;
			this._parameterAttributeCache = parameterAttributeCache;
			this._mapper = mapper;
            this._loggingService = loggingService;
            this._loggingService.CreateLogger(typeof(AssetSettingsListService));
			this._configurations = configurations.Value;
		}

        public async Task<AssetSettingsListResponse> FetchEssentialAssets(AssetSettingsListRequest request)
        {
            try
            {
				var deviceTypeParameterAttributes = await this._parameterAttributeCache.Get(request.DeviceType);			

				var validationResponse = await this.Validate<AssetSettingsListRequest>(_assetSettingsListValidators, request);
                var invalidRecords = validationResponse.Where(x => x.IsInvalid);
                if (invalidRecords.Any())
                {
                    this._loggingService.Info("Ignoring request since following records are invalid : " + JsonConvert.SerializeObject(invalidRecords), MethodInfo.GetCurrentMethod().Name);
                    throw new DomainException { Errors = validationResponse };
                }
                else
                {
                    this._loggingService.Debug("Building AssetTargetWorkDefinitionRequestDto", MethodInfo.GetCurrentMethod().Name);

                    var assetSettingsListRequestDto = new AssetSettingsListRequestDto
                    {
                        PageSize = request.PageSize,
                        PageNumber = request.PageNumber,
                        FilterName = request.FilterName,
                        FilterValue = request.FilterValue,
                        DeviceType = request.DeviceType,
                        SubAccountCustomerUid = request.SubAccountCustomerUid.HasValue ? request.SubAccountCustomerUid.Value.ToString("N") : null,
                        CustomerUid = request.CustomerUid.HasValue ? request.CustomerUid.Value.ToString("N") : null,
                        UserUid = request.UserUid.HasValue ? request.UserUid.Value.ToString("N") : null,
                        StatusInd = 1
                    };

                    if (!string.IsNullOrEmpty(request.SortColumn))
                    {
                        assetSettingsListRequestDto.SortDirection = request.SortColumn.StartsWith("-") ? "DESC" : "ASC";
                        request.SortColumn = request.SortColumn.Replace("-", string.Empty);
                        assetSettingsListRequestDto.SortColumn = request.SortColumn;
                    }

					await this.AssignDefaultValues(deviceTypeParameterAttributes, assetSettingsListRequestDto);

                    this._loggingService.Debug("Started Querying", MethodInfo.GetCurrentMethod().Name);
                    var essentialAssetsData = await this._assetSettingsListRepository.FetchEssentialAssets(assetSettingsListRequestDto);
                    this._loggingService.Debug("Ended Querying", MethodInfo.GetCurrentMethod().Name);

					await this.AssignTotalSwitchesCount(deviceTypeParameterAttributes, request, essentialAssetsData.Item2);

					var essentialAssets = this._mapper.Map<List<AssetSettingsDetails>>(essentialAssetsData.Item2);
                    var response = new AssetSettingsListResponse(essentialAssets, request.PageSize, request.PageNumber, essentialAssetsData.Item1);
                    response.Errors = validationResponse.OfType<ErrorInfo>().ToList();
                    return await Task.FromResult(response);                    
                }
            }
            catch(DomainException ex)
            {
                this._loggingService.Error("Error occurred in validation", MethodInfo.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

		private async Task AssignDefaultValues(IEnumerable<DeviceTypeParameterAttributeDto> deviceTypeParameterAttributes, AssetSettingsListRequestDto assetSettingsListRequestDto)
		{
			if (deviceTypeParameterAttributes != null && deviceTypeParameterAttributes.Any())
			{
				#region Moving Threshold
				var movingOrStoppedThresholdParameterAttributes = deviceTypeParameterAttributes.FirstOrDefault(x => x.ParameterName == "MovingOrStoppedThreshold");
				if (movingOrStoppedThresholdParameterAttributes != null)
				{
					var movingOrStoppedThreshold = JsonConvert.DeserializeObject<DeviceConfigurationDefaultsAndConfigurations>(movingOrStoppedThresholdParameterAttributes.DefaultValueJSON);
					assetSettingsListRequestDto.MovingOrStoppedThreshold = movingOrStoppedThreshold.Defaults.MovingThreshold.MovingOrStoppedThreshold;
				}

				var movingThresholdsRadiusParameterAttributes = deviceTypeParameterAttributes.FirstOrDefault(x => x.ParameterName == "MovingThresholdsRadius");
				if (movingThresholdsRadiusParameterAttributes != null)
				{
					var movingThresholdsRadius = JsonConvert.DeserializeObject<DeviceConfigurationDefaultsAndConfigurations>(movingThresholdsRadiusParameterAttributes.DefaultValueJSON);
					assetSettingsListRequestDto.MovingThresholdsRadius = movingThresholdsRadius.Defaults.MovingThreshold.MovingThresholdsRadius;
				}

				var movingThresholdsDurationParameterAttributes = deviceTypeParameterAttributes.FirstOrDefault(x => x.ParameterName == "MovingThresholdsDuration");
				if (movingThresholdsDurationParameterAttributes != null)
				{
					var movingThresholdsDuration = JsonConvert.DeserializeObject<DeviceConfigurationDefaultsAndConfigurations>(movingThresholdsDurationParameterAttributes.DefaultValueJSON);
					assetSettingsListRequestDto.MovingThresholdsDuration = movingThresholdsDuration.Defaults.MovingThreshold.MovingThresholdsDuration;
				}
				#endregion
			}
			await Task.Yield();
		}

		private async Task AssignTotalSwitchesCount(IEnumerable<DeviceTypeParameterAttributeDto> parameterAttributes, AssetSettingsListRequest request, IList<AssetSettingsListDto> assetSettingsListDtos)
		{
			if (parameterAttributes != null && parameterAttributes.Any())
			{
				var totalSwitchesCount = await this.GetTotalSwitchesCount(parameterAttributes, true, request.DeviceType);

				foreach (var assetSettingsListDto in assetSettingsListDtos)
				{
					assetSettingsListDto.TotalSwitches = totalSwitchesCount;
				}
			}
			await Task.Yield();
		}

		private async Task<int> GetTotalSwitchesCount(IEnumerable<DeviceTypeParameterAttributeDto> parameterAttributes, bool excludeTamperedSwitch, string deviceType)
		{
			int totalSwitchesCount = 0;

			var switchesParametersAttributes = parameterAttributes.Where(x => x.GroupName == "Switches").GroupBy(x => x.ParameterName);

			foreach (var switchParameterAttribute in switchesParametersAttributes)
			{
				var defaultAndConfigurations = JsonConvert.DeserializeObject<DeviceConfigurationDefaultsAndConfigurations>(switchParameterAttribute.FirstOrDefault().DefaultValueJSON);
				if (defaultAndConfigurations.Configurations != null && defaultAndConfigurations.Configurations.SwitchesConfig != null)
				{
					if (!(excludeTamperedSwitch && defaultAndConfigurations.Configurations.SwitchesConfig.isTampered))
					{
						totalSwitchesCount++;
					}
				}
			}
			return await Task.FromResult(totalSwitchesCount);
		}

        public async Task<IList<DeviceType>> FetchDeviceTypes(AssetDeviceTypeRequest request)
        {
            try
            {
                var validationResponse = await this.Validate<IServiceRequest>(_serviceRequestParametersValidators, request);
                var invalidRecords = validationResponse.Where(x => x.IsInvalid);
                IList<DeviceType> deviceTypes = new List<DeviceType>();
                if (invalidRecords.Any())
                {
                    this._loggingService.Info("Ignoring request since following records are invalid : " + JsonConvert.SerializeObject(invalidRecords), MethodInfo.GetCurrentMethod().Name);
                    throw new DomainException { Errors = validationResponse };
                }
                else
                {
                    this._loggingService.Debug("Building AssetTargetWorkDefinitionRequestDto", MethodInfo.GetCurrentMethod().Name);

                    request.StatusInd = 1;
                    request.UserGuid = request.UserUid.Value.ToString("N");
                    request.CustomerGuid = request.CustomerUid.Value.ToString("N");

                    var deviceTypeDtos = await this._assetSettingsListRepository.FetchDeviceTypesByAssetUID(request);
                    if (deviceTypeDtos != null && deviceTypeDtos.Item1 > 0)
                    {
                        deviceTypes = deviceTypeDtos.Item2.Select(x => new DeviceType
                        {
                            Id = x.DeviceType,
                            Name = x.DeviceType,
                            AssetCount = x.AssetCount
                        }).ToList();
                    }
                }
                return deviceTypes;
            }
            catch (DomainException ex)
            {
                this._loggingService.Error("Error occurred in validation", MethodInfo.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

        public async Task<IList<IErrorInfo>> Validate<T>(IEnumerable<IRequestValidator<T>> validators, T request) where T: IServiceRequest
        {
            List<IErrorInfo> errorInfos = new List<IErrorInfo>();

            this._loggingService.Debug("Started validation for the Asset Settings List request", MethodInfo.GetCurrentMethod().Name);

            //Validators
            foreach (var validator in validators)
            {
                var validationError = await validator.Validate(request);
                if (validationError != null)
                {
                    errorInfos.AddRange(validationError);
                }
            }
            this._loggingService.Debug("Ended validation for the Asset Settings List request", MethodInfo.GetCurrentMethod().Name);
            return errorInfos;
        }
    }
}
