using ClientModel.DeviceConfig.Request.DeviceConfig;
using CommonModel.Error;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Utilities.Logging;

namespace Infrastructure.Service.DeviceConfig.Validators
{
	public class AssetDeviceValidator : RequestValidatorBase, IRequestValidator<DeviceConfigRequestBase>
    {
        private readonly IAssetDeviceRepository _asssetDeviceRepo;

        public AssetDeviceValidator(ILoggingService loggingService, IAssetDeviceRepository asssetDeviceRepo) : base(loggingService)
        {
            _asssetDeviceRepo = asssetDeviceRepo;
        }

        public async Task<IList<IErrorInfo>> Validate(DeviceConfigRequestBase request)
        {
            IList<IErrorInfo> result = new List<IErrorInfo>();

            _loggingService.Info("Inside AssetDevice Validator", MethodBase.GetCurrentMethod().Name);


            if (request == null) { 
                result.Add(GetValidationResult(
                ErrorCodes.RequestNull,
                Utils.GetEnumDescription(ErrorCodes.RequestNull),
                true,
                "AssetDeviceValidator.Validate"));
                return result;
            }

            if (request != null && (request.AssetUIDs == null || request.AssetUIDs.Count==0)) { 
                result.Add(GetValidationResult(
                ErrorCodes.AssetUIDListNull,
                Utils.GetEnumDescription(ErrorCodes.AssetUIDListNull),
                true,
                "AssetDeviceValidator.Validate"));
                 return result;
            }


            var assetUIDDeviceUIDList = await _asssetDeviceRepo.Fetch(string.Join(",", request.AssetUIDs.Select(assetUID => "UNHEX('" + assetUID + "')").ToArray()));
            //No Device is associated with the asset
            if (!assetUIDDeviceUIDList.Any())
            {
                result = GetValidationResults(ErrorCodes.AssetDeviceMappingMissing,
                request.AssetUIDs.Select(assetUID => assetUID),
                Utils.GetEnumDescription(ErrorCodes.AssetDeviceMappingMissing),
                true,
                "AssetDeviceValidator.Validate");
                return result;
            }
            
            var invalidAssets = request.AssetUIDs.Except(assetUIDDeviceUIDList.Select(list => list.AssetID));

            if (invalidAssets.Any())
            {
                result = GetValidationResults(
                ErrorCodes.AssetDeviceMappingInvalid,
                invalidAssets,
                Utils.GetEnumDescription(ErrorCodes.AssetDeviceMappingInvalid),
                true,
                "AssetDeviceValidator.Validate");

                request.AssetUIDs.RemoveAll(invalidAssets.Contains);
            }
            return result.Count > 0 ? result : null;
        }
    }
}
