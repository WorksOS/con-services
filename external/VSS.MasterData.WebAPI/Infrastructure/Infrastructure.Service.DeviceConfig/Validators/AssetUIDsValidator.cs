using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Logging;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using CommonModel.Error;
using ClientModel.DeviceConfig.Request.DeviceConfig;
using Interfaces;
using DbModel.DeviceConfig;
using Infrastructure.Service.DeviceConfig.Interfaces;
using CommonModel.Helpers;

namespace Infrastructure.Service.DeviceConfig.Validators
{
    public class AssetUIDsValidator : RequestValidatorBase, IRequestValidator<DeviceConfigRequestBase>
    {
        private readonly IUserAssetRepository _userAssetRepository;

        public AssetUIDsValidator(IUserAssetRepository userAssetRepository, ILoggingService loggingService) : base(loggingService)
        {
            this._userAssetRepository = userAssetRepository;
        }

        public async Task<IList<IErrorInfo>> Validate(DeviceConfigRequestBase request)
        {
            var result = new List<IErrorInfo>();
            if (!request.IsAssetUIDValidationRequired)
            {
                return result;
            }
            if (request.AssetUIDs != null && request.AssetUIDs.Count() > 0)
            {
                if (request.CustomerUID.HasValue && request.UserUID.HasValue)
                {
                    var invalidGuids = new List<string>();
                    foreach(var assetUID in request.AssetUIDs)
                    {
                        Guid assetGuid;
                        if (!Guid.TryParse(assetUID, out assetGuid))
                        {
                            invalidGuids.Add(assetUID);
                        }
                    }

                    request.AssetUIDs.RemoveAll(invalidGuids.Contains);

                    if (invalidGuids.Count > 0)
                    {
                        result.Add(base.GetValidationResult(ErrorCodes.InvalidGuidFormatForAssetUID, string.Format(Utils.GetEnumDescription(ErrorCodes.InvalidGuidFormatForAssetUID), string.Join(",", invalidGuids)), true, "AssetUIDsValidator.Validate"));
                    }
                    if (request.AssetUIDs.Count > 0)
                    {
                        request.AssetUIDs = request.AssetUIDs.Select(x => Guid.Parse(x).ToStringWithoutHyphens().ToLower()).ToList();

                        var validAssetUids = await this._userAssetRepository.FetchValidAssetUIds(request.AssetUIDs, new UserAssetDto
                        {
                            CustomerUIDString = request.CustomerUID.Value.ToStringWithoutHyphens(),
                            UserUIDString = request.UserUID.Value.ToStringWithoutHyphens(),
                            TypeName = request.DeviceType
                        });

                        if (validAssetUids != null)
                        {
                            validAssetUids = validAssetUids.Select(x => x.ToLower()).ToList();
                        }

                        var invalidAssetUids = request.AssetUIDs.Except(validAssetUids).ToList();

                        if (invalidAssetUids.Count() > 0)
                        {
                            result.AddRange(base.GetValidationResults(ErrorCodes.InvalidAssetUID, invalidAssetUids, Utils.GetEnumDescription(ErrorCodes.InvalidAssetUID), false, "AssetUIDsValidator.Validate"));
                            request.AssetUIDs.RemoveAll(invalidAssetUids.Contains);
                        }
                    }
                }
            }
            else
            {
                result.Add(base.GetValidationResult(ErrorCodes.AssetUIDListNull, Utils.GetEnumDescription(ErrorCodes.AssetUIDListNull), false, "AssetUIDsValidator.Validate"));
            }
            return result.Count > 0 ? result : null;
        }
    }
}
