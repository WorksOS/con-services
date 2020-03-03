using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Logging;
using Infrastructure.Common.DeviceSettings.Enums;
using Infrastructure.Common.DeviceSettings.Helpers;
using CommonModel.Error;
using ClientModel.DeviceConfig.Request.DeviceConfig.AssetSecurity;
using ClientModel.DeviceConfig.Request.DeviceConfig.MaintenanceMode;
using Infrastructure.Service.DeviceConfig.Interfaces;

namespace Infrastructure.Service.DeviceConfig.Validators
{
    public class AssetSecurityValidator : RequestValidatorBase, IRequestValidator<DeviceConfigAssetSecurityRequest>
    {
        public AssetSecurityValidator(ILoggingService loggingService) : base(loggingService)
        {

        }

        public async Task<IList<IErrorInfo>> Validate(DeviceConfigAssetSecurityRequest request)
        {
            List<IErrorInfo> result = new List<IErrorInfo>();

            if (!request.SecurityStatus.HasValue || !Enum.IsDefined(typeof(AssetSecurityStatus), request.SecurityStatus.Value))
            {
                result.Add(base.GetValidationResult(ErrorCodes.AssetSecurityInvalidSecurityStatus, string.Format(Utils.GetEnumDescription(ErrorCodes.AssetSecurityInvalidSecurityStatus), Utils.GetEnumValuesAsKeyValueString(typeof(AssetSecurityStatus))), true, "AssetSecurityValidator.Validate"));
            }

            return result;
        }
    }
}
