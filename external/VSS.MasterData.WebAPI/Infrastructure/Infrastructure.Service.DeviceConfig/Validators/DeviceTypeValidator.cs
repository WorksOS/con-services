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
using Infrastructure.Cache.Interfaces;

namespace Infrastructure.Service.DeviceConfig.Validators
{
    public class DeviceTypeValidator : RequestValidatorBase, IRequestValidator<DeviceConfigRequestBase>
    {
        private readonly IDeviceTypeCache _deviceTypeCache;

        public DeviceTypeValidator(IDeviceTypeCache deviceTypeCache, ILoggingService loggingService) : base(loggingService)
        {
            this._deviceTypeCache = deviceTypeCache;
        }

        public async Task<IList<IErrorInfo>> Validate(DeviceConfigRequestBase request)
        {
            IList<IErrorInfo> errorInfo = new List<IErrorInfo>();
            if (request.DeviceType != null)
            {
                request.DeviceType = request.DeviceType.Trim();
            }
            if (string.IsNullOrEmpty(request.DeviceType))
            {
                errorInfo.Add(base.GetValidationResult(ErrorCodes.DeviceTypeNull, Utils.GetEnumDescription(ErrorCodes.DeviceTypeNull), true, "DeviceTypeValidator.Validate"));
            }
            else
            {
                var result = await this._deviceTypeCache.Get(request.DeviceType);
                if (result == null || !result.Any())
                {
                    errorInfo.Add(base.GetValidationResult(ErrorCodes.InvalidDeviceType, string.Format(Utils.GetEnumDescription(ErrorCodes.InvalidDeviceType), request.DeviceType), true, "DeviceTypeValidator.Validate"));
                }
            }
            return errorInfo;
        }
    }
}
