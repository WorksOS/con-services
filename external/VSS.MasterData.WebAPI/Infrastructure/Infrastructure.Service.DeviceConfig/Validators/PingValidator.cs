using CommonModel.DeviceSettings;
using ClientModel.DeviceConfig.Request;
using CommonModel.Error;
using DbModel.DeviceConfig;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using Infrastructure.Service.DeviceConfig.Interfaces;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilities.Logging;
using CommonModel.Helpers;

namespace Infrastructure.Service.DeviceConfig.Validators
{
	public class PingValidator : RequestValidatorBase, IRequestValidator<DevicePingLogRequest>
    {
        private readonly IDevicePingRepository _devicePingRepository;
        private readonly IUserAssetRepository _userAssetRepository;
        List<IErrorInfo> result = new List<IErrorInfo>();

        public PingValidator(ILoggingService loggingService, IDevicePingRepository devicePingRepository, IUserAssetRepository userAssetRepository) : base(loggingService)
        {
            this._devicePingRepository = devicePingRepository;
            this._userAssetRepository = userAssetRepository;
        }
        public async Task<IList<IErrorInfo>> Validate(DevicePingLogRequest request)
        {
            
            this._loggingService.Info("Starting validation for Ping Validator","PingValidator.Validate");

            if (request == null)
            {
                result.Add(GetValidationResult(
                ErrorCodes.RequestNull,
                Utils.GetEnumDescription(ErrorCodes.RequestNull),
                true,
                "PingValidator.Validate"));
                return result;
            }
            else if (request != null && request.AssetUID == Guid.Empty)
            {
                result.Add(GetValidationResult(
                ErrorCodes.AssetUIDIsNull,
                Utils.GetEnumDescription(ErrorCodes.AssetUIDIsNull),
                true,
                "PingValidator.Validate"));
                return result;
            }
            else if (request.DeviceUID == Guid.Empty)
            {
                result.Add(GetValidationResult(
                ErrorCodes.DeviceUIDIsNull,
                Utils.GetEnumDescription(ErrorCodes.DeviceUIDIsNull),
                true,
                "PingValidator.Validate"));
                return result;
            }
            else
            {
                DeviceTypeFamily deviceTypeFamily = await _devicePingRepository.GetDeviceTypeFamily(request.DeviceUID);

                if (deviceTypeFamily == null || !Constants.PING_ENABLED_DEVICE_TYPE_FAMILIES.ToUpper().Split(',').Contains(deviceTypeFamily.FamilyName.ToUpper()))
                {
                    result.Add(GetValidationResult(
                    ErrorCodes.DeviceTypeNotSupportedError,
                    Utils.GetEnumDescription(ErrorCodes.DeviceTypeNotSupportedError),
                    true,
                    "PingValidator.Validate"));
                    return result;
                }

                var validAssetUids = await this._userAssetRepository.FetchValidAssetUIds(new List<string>() { request.AssetUID.ToString() }, new UserAssetDto
                {
                    CustomerUIDString = request.CustomerUID.Value.ToStringWithoutHyphens(),
                    UserUIDString = request.UserUID.Value.ToStringWithoutHyphens(),
                    TypeName = deviceTypeFamily.TypeName
                });

                if (validAssetUids.Count() <= 0)
                {
                    result.Add(GetValidationResult(
                    ErrorCodes.AssetDeviceConfigNotExists,
                    Utils.GetEnumDescription(ErrorCodes.AssetDeviceConfigNotExists),
                    true,
                    "PingValidator.Validate"));
                    return result;
                }
            }
            this._loggingService.Info("Ended validation for Ping Validator", "PingValidator.Validate");
            return result;
        }
    }
}
