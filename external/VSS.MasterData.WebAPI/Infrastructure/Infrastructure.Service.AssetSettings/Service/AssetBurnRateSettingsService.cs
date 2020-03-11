using Interfaces;
using AutoMapper;
using ClientModel.AssetSettings.Request;
using ClientModel.AssetSettings.Request.AssetSettings;
using ClientModel.AssetSettings.Response;
using ClientModel.AssetSettings.Response.AssetTargets;
using CommonModel.Error;
using ClientModel.Interfaces;
using CommonModel.Enum;
using Infrastructure.Service.AssetSettings.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;

namespace Infrastructure.Service.AssetSettings.Service
{
	public class AssetBurnRateSettingsService : AssetSettingsServiceBase, IAssetSettingsService<AssetFuelBurnRateSettingRequest, AssetFuelBurnRateSettingsDetails>
    {
        private readonly IEnumerable<IRequestValidator<AssetSettingsRequestBase>> _assetSettingsValidators;
        private readonly IEnumerable<IRequestValidator<IServiceRequest>> _serviceRequestParametersValidators;
        private readonly IEnumerable<IRequestValidator<AssetFuelBurnRateSettingRequest>> _assetBurnRateSettingsValidators;

        public AssetBurnRateSettingsService(IAssetConfigRepository assetSettingsRepository,
            IAssetConfigTypeRepository assetConfigTypeRepository,
            IAssetSettingsPublisher assetSettingsPublisher,
            IEnumerable<IRequestValidator<AssetSettingsRequestBase>> assetSettingsValidators,
            IEnumerable<IRequestValidator<AssetFuelBurnRateSettingRequest>> assetBurnRateSettingsValidators,
            IEnumerable<IRequestValidator<IServiceRequest>> serviceRequestParametersValidators,
            IMapper mapper,
			ITransactions transaction,
			ILoggingService loggingService) : base(assetSettingsRepository, assetConfigTypeRepository, assetSettingsPublisher,  mapper, transaction, loggingService)
        {
            this._assetSettingsValidators = assetSettingsValidators;
            this._serviceRequestParametersValidators = serviceRequestParametersValidators;
            this._assetBurnRateSettingsValidators = assetBurnRateSettingsValidators;
        }

        public async Task<AssetSettingsServiceResponse<AssetFuelBurnRateSettingsDetails>> Fetch(AssetFuelBurnRateSettingRequest burnRateRequest)
        {
            List<IErrorInfo> errorsInfo = new List<IErrorInfo>();

            errorsInfo.AddRange(await base.Validate<AssetSettingsRequestBase>(_assetSettingsValidators, burnRateRequest));
            errorsInfo.AddRange(await base.Validate(_serviceRequestParametersValidators, burnRateRequest));

            base.CheckForInvalidRecords(burnRateRequest, errorsInfo);

            var response = await base.Fetch(burnRateRequest, errorsInfo);

            return new AssetSettingsServiceResponse<AssetFuelBurnRateSettingsDetails> { AssetSettingsLists = this.BuildResponse(response), Errors = errorsInfo };
        }

        public async Task<AssetSettingsServiceResponse<AssetFuelBurnRateSettingsDetails>> Save(AssetFuelBurnRateSettingRequest burnRateRequest)
        {
            List<IErrorInfo> errorsInfo = new List<IErrorInfo>();

            errorsInfo.AddRange(await base.Validate<AssetFuelBurnRateSettingRequest>(_assetBurnRateSettingsValidators, burnRateRequest));
            errorsInfo.AddRange(await base.Validate<AssetSettingsRequestBase>(_assetSettingsValidators, burnRateRequest));
            errorsInfo.AddRange(await base.Validate(_serviceRequestParametersValidators, burnRateRequest));

            base.CheckForInvalidRecords(burnRateRequest, errorsInfo);

            var response = await base.Save(burnRateRequest, errorsInfo);

            return new AssetSettingsServiceResponse<AssetFuelBurnRateSettingsDetails> { AssetSettingsLists = this.BuildResponse(response), Errors = errorsInfo };
        }

        private IList<AssetFuelBurnRateSettingsDetails> BuildResponse(IList<AssetSettingsResponse> response)
        {
            IList<AssetFuelBurnRateSettingsDetails> assetBurnRateSettingDetails = new List<AssetFuelBurnRateSettingsDetails>();

            foreach (var assetGroups in response.GroupBy(x => x.AssetUID))
            {
                var assetBurnRateSettingDetail = new AssetFuelBurnRateSettingsDetails
                {
                    AssetUID = assetGroups.Key
                };
                foreach (var assetsInGroup in assetGroups)
                {
                    switch (assetsInGroup.TargetType)
                    {
                        case AssetTargetType.IdlingBurnRateinLiPerHour:
                            assetBurnRateSettingDetail.IdleBurnRateTargetValue = assetsInGroup.TargetValue;
                            break;
                        case AssetTargetType.WorkingBurnRateinLiPerHour:
                            assetBurnRateSettingDetail.WorkBurnRateTargetValue = assetsInGroup.TargetValue;
                            assetBurnRateSettingDetail.StartDate = assetsInGroup.StartDate;
                            assetBurnRateSettingDetail.EndDate = assetsInGroup.EndDate;
                            break;
                    }
                }
                assetBurnRateSettingDetails.Add(assetBurnRateSettingDetail);
            }
            return assetBurnRateSettingDetails;
        }
    }
}
