using Interfaces;
using ClientModel.AssetSettings.Request;
using CommonModel.Error;
using DbModel.AssetSettings;
using CommonModel.Enum;
using Infrastructure.Common.Helpers;
using Infrastructure.Service.AssetSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.Logging;

namespace Infrastructure.Service.AssetSettings.Validators
{
	public class AssetUIDsValidator : RequestValidatorBase, IRequestValidator<AssetSettingsRequestBase>
    {
        private readonly IAssetSettingsListRepository _assetSettingsListRepository;

        public AssetUIDsValidator(IAssetSettingsListRepository assetSettingsListRepository, ILoggingService loggingService) : base(loggingService)
        {
            this._assetSettingsListRepository = assetSettingsListRepository;
        }

        public async Task<IList<IErrorInfo>> Validate(AssetSettingsRequestBase request)
        {
            var result = new List<IErrorInfo>();

			if (request.AssetUIds != null && request.AssetUIds.Count() > 0)
            {
                if (request.CustomerUid.HasValue && request.UserUid.HasValue)
                {
                    var validAssetUids = await this._assetSettingsListRepository.FetchValidAssetUIds(request.AssetUIds, new AssetSettingsListRequestDto
                    {
                        CustomerUid = request.CustomerUid.Value.ToString("N"),
                        UserUid = request.UserUid.Value.ToString("N"),
                        StatusInd = 1
                    });

                    if (validAssetUids != null)
                    {
                        validAssetUids = validAssetUids.Select(x => Guid.Parse(x).ToString());
					}
					var invalidAssetUids = request.AssetUIds.Except(validAssetUids);

					if (invalidAssetUids.Count() > 0)
					{
						result.AddRange(base.GetValidationResults(ErrorCodes.InvalidAssetUID, invalidAssetUids, UtilHelpers.GetEnumDescription(ErrorCodes.InvalidAssetUID), false, MethodInfo.GetCurrentMethod().Name));
						request.AssetUIds.RemoveAll(invalidAssetUids.Contains);
					}
				}
            }
            else
            {
                result.Add(base.GetValidationResult(ErrorCodes.AssetUIDListNull, UtilHelpers.GetEnumDescription(ErrorCodes.AssetUIDListNull), false, MethodInfo.GetCurrentMethod().Name));
            }
            return result.Count > 0 ? result : null;
        }
    }
}
