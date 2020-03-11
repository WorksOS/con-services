using Interfaces;
using AutoMapper;
using ClientModel.AssetSettings.Request;
using ClientModel.AssetSettings.Response.AssetTargets;
using CommonModel.Error;
using CommonModel.Exceptions;
using ClientModel.Interfaces;
using DbModel.AssetSettings;
using CommonModel.Enum;
using Infrastructure.Common.Helpers;
using Infrastructure.Service.AssetSettings.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;

namespace Infrastructure.Service.AssetSettings.Service
{
	public abstract class AssetSettingsServiceBase
    {
        protected readonly IMapper _mapper;
        protected readonly ILoggingService _loggingService;
        private readonly IAssetConfigRepository _assetSettingsRepository;
        private readonly IAssetConfigTypeRepository _assetConfigTypeRepository;
        private readonly IAssetSettingsPublisher _assetSettingsPublisher;
        private readonly ITransactions _transactions;

        public AssetSettingsServiceBase(IAssetConfigRepository assetSettingsRepository,
            IAssetConfigTypeRepository assetConfigTypeRepository,
            IAssetSettingsPublisher assetSettingsPublisher,
            IMapper mapper,
			ITransactions transactions,
            ILoggingService loggingService)
        {
            this._assetSettingsRepository = assetSettingsRepository;
            this._assetConfigTypeRepository = assetConfigTypeRepository;
            this._assetSettingsPublisher = assetSettingsPublisher;
            this._mapper = mapper;
            this._transactions = transactions;
            this._loggingService = loggingService;
            this._loggingService.CreateLogger<AssetSettingsServiceBase>();
        }

        protected async Task<IList<IErrorInfo>> Validate<T>(IEnumerable<IRequestValidator<T>> validators, T request) where T : IServiceRequest
        {
            this._loggingService.Debug("Started validation for the Asset Settings Request", "AssetSettingsServiceBase.Validate");

            List<IErrorInfo> errorInfos = new List<IErrorInfo>();
            foreach (var validator in validators)
            {
                var validationResult = await validator.Validate(request);
                if (validationResult == null)
                {
                    continue;
                }
                errorInfos.AddRange(validationResult);
            }

            this._loggingService.Debug("Ended validation for the Asset Settings Request", "AssetSettingsServiceBase.Validate");

            return errorInfos;
        }

        protected virtual void CheckForInvalidRecords(AssetSettingsRequestBase request, List<IErrorInfo> errorInfos)
        {
            var invalidRecords = errorInfos.Where(x => x.IsInvalid);

            if (errorInfos.Where(x => x.IsInvalid).Any())
            {
                this._loggingService.Info("Ignoring request since following records are invalid : " + JsonConvert.SerializeObject(invalidRecords), MethodInfo.GetCurrentMethod().Name);
                throw new DomainException { Errors = errorInfos };
            }

            if (request.AssetUIds == null || !request.AssetUIds.Any())
            {
                throw new DomainException
				{
                    Errors = errorInfos.Any() ? errorInfos : new List<IErrorInfo>
					{
                        new ErrorInfo
						{
                            ErrorCode = (int)ErrorCodes.AssetUIDListNull,
                            Message = UtilHelpers.GetEnumDescription(ErrorCodes.AssetUIDListNull)
                        }
                    }
                };
            }
        }

        protected virtual async Task<IEnumerable<AssetSettingsDto>> FetchAssetConfig(AssetSettingsRequestBase request, string startDateComparer)
        {
            this._loggingService.Debug("Started fetching AssetConfig details from database", MethodInfo.GetCurrentMethod().Name);
            var assetsSettingsResponse = await this._assetSettingsRepository.FetchAssetConfig(request.AssetUIds, new AssetSettingsDto
            {
                StartDate = request.StartDate,
                TargetValues = this.AssignAssetTargetValues(request.TargetValues, new AssetSettingsDto()),
                FilterCriteria = new List<KeyValuePair<string, Tuple<string, object>>>
                {
                    new KeyValuePair<string, Tuple<string, object>>(startDateComparer, new Tuple<string, object>("AC.StartDate", request.StartDate.ToDateTimeStringWithYearMonthDayFormat())),
                    new KeyValuePair<string, Tuple<string, object>>("is", new Tuple<string, object>("AC.EndDate", null))
                }
            });
            this._loggingService.Debug("Ended fetching AssetConfig details from database", MethodInfo.GetCurrentMethod().Name);
            return assetsSettingsResponse;
        }

        private IEnumerable<AssetSettingsDto> BuildAssetConfig(List<AssetSettingsDto> assetSettingsList, AssetSettingsRequestBase request, Dictionary<AssetTargetType, int> configTypeDictionary)
        {
			DateTime currentDateTime = DateTime.UtcNow;

            List<AssetSettingsDto> resultAssetSettings = new List<AssetSettingsDto>();

            //Build Asset UIDs to be created 
            var assetUidsToBeCreated = request.AssetUIds.Except(assetSettingsList.Distinct().Select(x => x.AssetUID.ToString())).ToList();

            this._loggingService.Info("Following AssetUIDs are not having opened AssetConfig records  : " + string.Join(",", assetUidsToBeCreated), MethodInfo.GetCurrentMethod().Name);

            if (assetUidsToBeCreated.Count > 0)
            {
                foreach (var requestTargetType in request.TargetValues)
                {
                    resultAssetSettings.AddRange(assetUidsToBeCreated.Select(x => new AssetSettingsDto
                    {
                        AssetConfigUID = Guid.NewGuid(),
                        AssetUID = Guid.Parse(x),
                        StartDate = request.StartDate,
                        TargetType = requestTargetType.Key.ToString(),
                        TargetValue = requestTargetType.Value ?? 0,
                        AssetConfigTypeID = configTypeDictionary[requestTargetType.Key],
                        InsertUTC = currentDateTime,
                        UpdateUTC = currentDateTime,
                        StatusInd = true
                    }));
                }
            }

            foreach (var assetConfig in assetSettingsList)
            {
                AssetTargetType targetType = (AssetTargetType)Enum.Parse(typeof(AssetTargetType), assetConfig.TargetType);
				assetConfig.AssetConfigTypeID = configTypeDictionary[targetType];

				assetConfig.UpdateUTC = currentDateTime;
                //Unsetting the current config
                if (!request.TargetValues[targetType].HasValue)
                {
                    _loggingService.Info("Assigning EndDate as Target values are null for the Asset : " + assetConfig.AssetUID, "AssetSettingsServiceBase.BuildAssetConfig");
                    if (Convert.ToDateTime(assetConfig.StartDate) == request.StartDate)
                        assetConfig.EndDate = request.StartDate;
                    else
                        assetConfig.EndDate = request.StartDate.AddDays(-1);
                    _loggingService.Info("Assigned EndDate : " + assetConfig.EndDate + " for the Asset : " + assetConfig.AssetUID, "AssetSettingsServiceBase.BuildAssetConfig");
                }
                //Check if StartDate < request.StartDate ie., Old Records
                else if (Convert.ToDateTime(assetConfig.StartDate) < request.StartDate)
                {
                    _loggingService.Info("Assigning EndDate as user is updating on next day for the Asset : " + assetConfig.AssetUID, "AssetSettingsServiceBase.BuildAssetConfig");
                    assetConfig.EndDate = request.StartDate.AddDays(-1);
                    this._loggingService.Info("Assigned EndDate : " + assetConfig.EndDate + " for the Asset : " + assetConfig.AssetUID, "AssetSettingsServiceBase.BuildAssetConfig");

                    resultAssetSettings.Add(new AssetSettingsDto
                    {
                        AssetConfigUID = Guid.NewGuid(),
                        AssetUID = assetConfig.AssetUID,
                        TargetType = assetConfig.TargetType,
                        TargetValue = Convert.ToDouble(request.TargetValues[targetType]),
                        InsertUTC = currentDateTime,
                        StartDate = request.StartDate,
                        AssetConfigTypeID = configTypeDictionary[targetType],
                        UpdateUTC = currentDateTime,
                        StatusInd = true
                    });
                    this._loggingService.Info("Created new AssetConfig with StartDate : " + assetConfig.StartDate + " for the Asset : " + assetConfig.AssetUID, "AssetSettingsServiceBase.BuildAssetConfig");
                }
                else
                {
                    //Assign Target value to the given target value ie., for updation
                    assetConfig.TargetValue = Convert.ToDouble(request.TargetValues[targetType]);
                    this._loggingService.Info("Assigned TargetValue : " + assetConfig.TargetValue + " for the Asset : " + assetConfig.AssetUID, "AssetSettingsServiceBase.BuildAssetConfig");
                }
            }
            resultAssetSettings.AddRange(assetSettingsList);
            return resultAssetSettings;
        }

        private bool PersistAndPublish(IEnumerable<AssetSettingsDto> assetSettings, AssetSettingsRequestBase request)
        {
            bool result = true;
			List<Action> actions = new List<Action>();
			if (assetSettings.Any())
			{
				actions.Add(() => _transactions.Upsert(assetSettings));
				actions.Add(() => this._assetSettingsPublisher.PublishAssetSettings(assetSettings));
				actions.Add(() => this._assetSettingsPublisher.PublishUserAssetSettings(request)); //history record

				result = _transactions.Execute(actions);
			}
            
            return result;
        }


        private IDictionary<AssetTargetType, Tuple<Guid, double>> AssignAssetTargetValues(IDictionary<AssetTargetType, double?> requestTargetValues, AssetSettingsDto asset)
        {
            IDictionary<AssetTargetType, Tuple<Guid, double>> targetValues = new Dictionary<AssetTargetType, Tuple<Guid, double>>();
            if (asset.TargetValues == null)
            {
                asset.TargetValues = new Dictionary<AssetTargetType, Tuple<Guid, double>>();
            }
            foreach (var targetValue in requestTargetValues)
            {
                Tuple<Guid, double> tuple = new Tuple<Guid, double>(Guid.Empty, 0);
                if (asset.TargetValues.ContainsKey(targetValue.Key))
                {
                    tuple = new Tuple<Guid, double>(asset.TargetValues[targetValue.Key].Item1, Convert.ToDouble(targetValue.Value));
                }
                targetValues.Add(targetValue.Key, tuple);
            }
            return targetValues;
        }

        private IEnumerable<AssetSettingsDto> GroupAssetSttingsByUID(List<AssetSettingsDto> assetsSettingsList)
        {
            var groupedAssetSettingsList = new ConcurrentDictionary<Guid, AssetSettingsDto>();

            Parallel.ForEach(assetsSettingsList.GroupBy(x => x.AssetUID), assetPerUID =>
            {
                foreach (var asset in assetPerUID)
                {
                    var targetType = (AssetTargetType)Enum.Parse(typeof(AssetTargetType), asset.TargetType, true);
                    var targetValue = new KeyValuePair<AssetTargetType, Tuple<Guid, double>>(targetType, new Tuple<Guid, double>(asset.AssetConfigUID, asset.TargetValue));
                    if (!groupedAssetSettingsList.ContainsKey(asset.AssetUID))
                    {
                        asset.TargetValues = new Dictionary<AssetTargetType, Tuple<Guid, double>> { { targetValue.Key, new Tuple<Guid, double>(asset.AssetConfigUID, asset.TargetValue) } };
                        groupedAssetSettingsList.TryAdd(asset.AssetUID, asset);
                    }
                    else
                    {
                        var groupedAsset = groupedAssetSettingsList[asset.AssetUID];
                        if (groupedAsset.TargetValues == null)
                        {
                            groupedAsset.TargetValues = new Dictionary<AssetTargetType, Tuple<Guid, double>> { { targetValue.Key, new Tuple<Guid, double>(asset.AssetConfigUID, asset.TargetValue) } };
                        }
                        else
                        {
                            groupedAsset.TargetValues.Add(targetValue);
                        }
                        groupedAssetSettingsList[asset.AssetUID] = groupedAsset;
                    }
                }
            });

            return groupedAssetSettingsList.Values;
        }


        public virtual async Task<IList<AssetSettingsResponse>> Fetch(AssetSettingsRequestBase request, IList<IErrorInfo> errorInfos)
        {
            List<AssetSettingsResponse> convertedResponse = new List<AssetSettingsResponse>();
            try
            {
                //Fetch from DB
                var results = await this.FetchAssetConfig(request, "<=");

                if (results.Any())
                {
                    convertedResponse.AddRange(this._mapper.Map<IEnumerable<AssetSettingsResponse>>(results));
                }
            }
            catch (DomainException domainException)
            {
                this._loggingService.Error("An Error occurred during validation", MethodInfo.GetCurrentMethod().Name, domainException);
                throw domainException;
            }
            catch (Exception ex)
            {
                this._loggingService.Error("An Exception has occurred", MethodInfo.GetCurrentMethod().Name, ex);
                throw ex;
            }
            return convertedResponse;
        }

        public virtual async Task<IList<AssetSettingsResponse>> Save(AssetSettingsRequestBase request, IList<IErrorInfo> errorInfos)
        {
            List<AssetSettingsResponse> convertedResponse = new List<AssetSettingsResponse>();
            try
            {
                //Fetch from DB
                var assetsSettingsResponse = await this.FetchAssetConfig(request, "<=");

                var assetsSettingsList = assetsSettingsResponse.ToList();

                var configTypeIds = this._assetConfigTypeRepository.FetchByConfigTypeNames(
                                    new AssetConfigTypeDto
                                    {
                                        ConfigTypeNames = request.TargetValues.Select(x => x.Key.ToString())
                                    }).Result;

                var configTypeDictionary = configTypeIds.ToDictionary(x => x.AssetTargetType, x => x.AssetConfigTypeID);

                this._loggingService.Debug("Started parallel execution for insertion / updation", MethodInfo.GetCurrentMethod().Name);

                var updatedAssetSettingsList = this.BuildAssetConfig(assetsSettingsList, request, configTypeDictionary);

                this.PersistAndPublish(updatedAssetSettingsList, request);

                this._loggingService.Debug("Ended parallel execution for insertion / updation", MethodInfo.GetCurrentMethod().Name);


                //Fetch from DB for the updated values
                var results = await this.FetchAssetConfig(request, "=");

                if (results.Any())
                {
                    convertedResponse.AddRange(this._mapper.Map<List<AssetSettingsResponse>>(results).ToArray());
                }
            }
            catch (DomainException domainException)
            {
                this._loggingService.Error("An Error occurred during validation", MethodInfo.GetCurrentMethod().Name, domainException);
                throw domainException;
            }
            catch (Exception ex)
            {
                this._loggingService.Error("An Exception has occurred", MethodInfo.GetCurrentMethod().Name, ex);
                throw ex;
            }
            return convertedResponse;
        }
    }
}
