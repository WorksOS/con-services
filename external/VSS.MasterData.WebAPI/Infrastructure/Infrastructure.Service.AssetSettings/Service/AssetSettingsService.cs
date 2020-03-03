using Interfaces;
using AutoMapper;
using ClientModel.AssetSettings.Request;
using ClientModel.AssetSettings.Response;
using ClientModel.AssetSettings.Response.AssetTargets;
using CommonModel.Error;
using ClientModel.Interfaces;
using Infrastructure.Service.AssetSettings.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilities.Logging;
using VSS.MasterData.WebAPI.Transactions;
//using VSS.DB.Interfaces;

namespace Infrastructure.Service.AssetSettings.Service
{
	public class AssetSettingsService : AssetSettingsServiceBase, IAssetSettingsService<AssetSettingsRequestBase, AssetSettingsResponse>
    {
        private readonly IEnumerable<IRequestValidator<AssetSettingsRequestBase>> _assetSettingsValidators;
        private readonly IEnumerable<IRequestValidator<IServiceRequest>> _serviceRequestParametersValidators;


        public AssetSettingsService(IAssetConfigRepository assetSettingsRepository,
            IAssetConfigTypeRepository assetConfigTypeRepository,
            IAssetSettingsPublisher assetSettingsPublisher,
            IEnumerable<IRequestValidator<AssetSettingsRequestBase>> assetSettingsValidators,
            IEnumerable<IRequestValidator<IServiceRequest>> serviceRequestParametersValidators,
            IMapper mapper,
            ITransactions transactions,
            ILoggingService loggingService) : base(assetSettingsRepository, assetConfigTypeRepository, assetSettingsPublisher, mapper, transactions, loggingService)
        {
            this._assetSettingsValidators = assetSettingsValidators;
            this._serviceRequestParametersValidators = serviceRequestParametersValidators;
        }

        public async Task<AssetSettingsServiceResponse<AssetSettingsResponse>> Fetch(AssetSettingsRequestBase request)
        {
            List<IErrorInfo> errorsInfo = new List<IErrorInfo>();
            errorsInfo.AddRange(await base.Validate<AssetSettingsRequestBase>(_assetSettingsValidators, request));
            errorsInfo.AddRange(await base.Validate(_serviceRequestParametersValidators, request));

            base.CheckForInvalidRecords(request, errorsInfo);

            var responses = await base.Fetch(request, errorsInfo);
            return new AssetSettingsServiceResponse<AssetSettingsResponse> { AssetSettingsLists = responses, Errors = errorsInfo };
        }

        public async Task<AssetSettingsServiceResponse<AssetSettingsResponse>> Save(AssetSettingsRequestBase request)
        {
            List<IErrorInfo> errorsInfo = new List<IErrorInfo>();
            errorsInfo.AddRange(await base.Validate<AssetSettingsRequestBase>(_assetSettingsValidators, request));
            errorsInfo.AddRange(await base.Validate(_serviceRequestParametersValidators, request));

            base.CheckForInvalidRecords(request, errorsInfo);

            var responses = await base.Save(request, errorsInfo);
            return new AssetSettingsServiceResponse<AssetSettingsResponse> { AssetSettingsLists = responses, Errors = errorsInfo };
        }
    }
}
